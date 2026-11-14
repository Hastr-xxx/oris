using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MiniTemplateEngine
{
    public class HtmlTemplateRenderer : IHtmlTemplateRenderer
    {
        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            return Parse(htmlTemplate, dataModel, null);
        }

        public string RenderFromFile(string filePath, object dataModel)
        {
            string template = File.ReadAllText(filePath);
            return RenderFromString(template, dataModel);
        }

        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            string result = RenderFromFile(inputFilePath, dataModel);
            File.WriteAllText(outputFilePath, result);
            return result;
        }

        private string Parse(string template, object model, Dictionary<string, object> locals)
        {
            StringBuilder result = new StringBuilder();
            int i = 0;

            while (i < template.Length)
            {
                int tagStart = template.IndexOf("${", i);
                if (tagStart == -1)
                {
                    result.Append(template.Substring(i));
                    break;
                }

                result.Append(template.Substring(i, tagStart - i));

                int tagEnd = template.IndexOf("}", tagStart);
                if (tagEnd == -1)
                {
                    result.Append(template.Substring(tagStart));
                    break;
                }

                string tagContent = template.Substring(tagStart + 2, tagEnd - tagStart - 2).Trim();

                if (tagContent.StartsWith("if "))
                {
                    int endifIndex = FindMatchingEnd(template, tagStart, "${if ", "${endif}");
                    if (endifIndex == -1) throw new Exception("Ошибка: не найден ${endif}");

                    int elseIndex = FindElse(template, tagStart, endifIndex);

                    string conditionStr = tagContent.Substring(3).Trim();
                    var evalResult = EvaluateExpression(conditionStr, model, locals);
                    bool condition = evalResult is bool b && b;

                    string trueContent = "";
                    string falseContent = "";

                    if (elseIndex != -1)
                    {
                        int elseEnd = template.IndexOf("}", elseIndex) + 1;
                        trueContent = template.Substring(tagEnd + 1, elseIndex - tagEnd - 1);
                        falseContent = template.Substring(elseEnd, endifIndex - elseEnd);
                    }
                    else
                    {
                        trueContent = template.Substring(tagEnd + 1, endifIndex - tagEnd - 1);
                    }

                    string blockContent = condition ? trueContent : falseContent;
                    result.Append(Parse(blockContent, model, locals));
                    i = endifIndex + "${endif}".Length;
                }
                else if (tagContent.StartsWith("for "))
                {
                    int endforIndex = FindMatchingEnd(template, tagStart, "${for ", "${endfor}");
                    if (endforIndex == -1) throw new Exception("Ошибка: не найден ${endfor}");

                    string forExpr = tagContent.Substring(4).Trim();
                    string[] parts = forExpr.Split(new[] { " in " }, StringSplitOptions.None);
                    if (parts.Length != 2) throw new Exception("Неверный синтаксис for. Используйте: for item in Collection");

                    string itemName = parts[0].Trim();
                    string collectionExpr = parts[1].Trim();

                    var collection = EvaluateExpression(collectionExpr, model, locals) as IEnumerable;
                    if (collection != null)
                    {
                        string blockContent = template.Substring(tagEnd + 1, endforIndex - tagEnd - 1);
                        foreach (var item in collection)
                        {
                            var newLocals = locals != null ? new Dictionary<string, object>(locals) : new Dictionary<string, object>();
                            newLocals[itemName] = item;

                            result.Append(Parse(blockContent, model, newLocals));
                        }
                    }
                    i = endforIndex + "${endfor}".Length;
                }
                else
                {
                    var val = EvaluateExpression(tagContent, model, locals);
                    result.Append(val?.ToString() ?? "");
                    i = tagEnd + 1;
                }
            }

            return result.ToString();
        }

        private int FindMatchingEnd(string template, int startIndex, string startTag, string endTag)
        {
            int depth = 1;
            int index = startIndex + startTag.Length;
            while (index < template.Length)
            {
                int nextStart = template.IndexOf(startTag, index);
                int nextEnd = template.IndexOf(endTag, index);
                if (nextEnd == -1) return -1;

                if (nextStart != -1 && nextStart < nextEnd)
                {
                    depth++;
                    index = nextStart + startTag.Length;
                }
                else
                {
                    depth--;
                    if (depth == 0) return nextEnd;
                    index = nextEnd + endTag.Length;
                }
            }
            return -1;
        }


        private int FindElse(string template, int startIndex, int endIndex)
        {
            int depth = 1;
            int index = startIndex + 2;
            while (index < endIndex)
            {
                int nextStart = template.IndexOf("${if ", index);
                int nextEnd = template.IndexOf("${endif}", index);
                int nextElse = template.IndexOf("${else}", index);

                int earliest = -1;
                if (nextStart != -1 && nextStart < endIndex) earliest = nextStart;
                if (nextEnd != -1 && nextEnd < endIndex && (earliest == -1 || nextEnd < earliest)) earliest = nextEnd;
                if (nextElse != -1 && nextElse < endIndex && (earliest == -1 || nextElse < earliest)) earliest = nextElse;

                if (earliest == -1) break;

                if (earliest == nextStart)
                {
                    depth++;
                    index = nextStart + 2;
                }
                else if (earliest == nextEnd)
                {
                    depth--;
                    index = nextEnd + 2;
                }
                else if (earliest == nextElse)
                {
                    if (depth == 1) return nextElse;
                    index = nextElse + 2;
                }
            }
            return -1;
        }


        private object EvaluateExpression(string expression, object model, Dictionary<string, object> locals)
        {
            if (expression == "true") return true;
            if (expression == "false") return false;

            string[] parts = expression.Split('.');
            object current = null;

            string root = parts[0];

            if (locals != null && locals.ContainsKey(root))
            {
                current = locals[root];
            }
            else
            {
                if (model != null && model.GetType().Name.Equals(root, StringComparison.OrdinalIgnoreCase))
                {
                    current = model;
                }
                else
                {
                    current = GetPropertyValue(model, root);
                }
            }

            for (int i = 1; i < parts.Length; i++)
            {
                current = GetPropertyValue(current, parts[i]);
            }

            return current;
        }


        private object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;
            var type = obj.GetType();

            var prop = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(obj);

            var field = type.GetField(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(obj);

            return null;
        }
    }
}