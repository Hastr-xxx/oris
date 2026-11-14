using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Framework.Core;

namespace CustomHttpServer.Framework.Handlers
{
    public class ControllerHandler : Handler
    {
        private readonly Dictionary<string, (Type Controller, MethodInfo Method)> routes = new();

        public ControllerHandler()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                var controllerAttr = type.GetCustomAttribute<HttpControllerAttribute>();
                if (controllerAttr == null) continue;

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var getAttr = method.GetCustomAttribute<GetAttribute>();
                    var postAttr = method.GetCustomAttribute<PostAttribute>();
                    var putAttr = method.GetCustomAttribute<PutAttribute>();

                    string controllerRoute = controllerAttr.Route.Trim('/');
                    string methodRoute = getAttr?.Route?.Trim('/') ?? postAttr?.Route?.Trim('/') ?? putAttr?.Route?.Trim('/') ?? "";
                    string fullRoute = string.IsNullOrEmpty(methodRoute) ? $"/{controllerRoute}" : $"/{controllerRoute}/{methodRoute}";

                    if (getAttr != null) routes[$"GET:{fullRoute}".ToLowerInvariant()] = (type, method);
                    if (postAttr != null) routes[$"POST:{fullRoute}".ToLowerInvariant()] = (type, method);
                    if (putAttr != null) routes[$"PUT:{fullRoute}".ToLowerInvariant()] = (type, method);
                }
            }
        }

        public override async Task HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url?.LocalPath ?? "/";

            if (path.Length > 1 && path.EndsWith("/"))
                path = path.TrimEnd('/');

            string method = context.Request.HttpMethod.ToUpperInvariant();
            string key = $"{method}:{path}".ToLowerInvariant();

            if (routes.TryGetValue(key, out var route))
            {
                await ExecuteAction(route.Controller, route.Method, context);
                return;
            }

            if (Successor != null) await Successor.HandleRequest(context);
            else { context.Response.StatusCode = 404; context.Response.Close(); }
        }

        private async Task ExecuteAction(Type controllerType, MethodInfo method, HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            object controllerInstance = Activator.CreateInstance(controllerType)!;
            var parameters = method.GetParameters();
            object?[] args = new object?[parameters.Length];

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string? key in request.QueryString.AllKeys)
                if (!string.IsNullOrEmpty(key)) values[key] = request.QueryString[key] ?? "";

            if ((request.HttpMethod == "POST" || request.HttpMethod == "PUT") && request.HasEntityBody)
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string body = await reader.ReadToEndAsync();
                foreach (var kv in ParseFormData(body)) values[kv.Key] = kv.Value;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                string name = parameters[i].Name!;
                if (values.TryGetValue(name, out var value)) args[i] = value;
                else args[i] = parameters[i].ParameterType.IsValueType ? Activator.CreateInstance(parameters[i].ParameterType) : null;
            }

            object? result = method.Invoke(controllerInstance, args);

            if (result is Task task)
            {
                await task;
                var resultProperty = task.GetType().GetProperty("Result");
                result = resultProperty?.GetValue(task);
            }

            await ProcessResult(result, response);
        }

        private async Task ProcessResult(object? result, HttpListenerResponse response)
        {
            response.ContentType = "text/html; charset=utf-8";
            byte[] buffer;
            string text = "";

            switch (result)
            {
                case HtmlResult htmlRes:
                    response.StatusCode = 200;
                    text = htmlRes.Html;
                    break;
                case RedirectResult redirectRes:
                    response.StatusCode = 303;
                    response.RedirectLocation = redirectRes.Url;
                    response.Close();
                    return;
                case NoContentResult:
                    response.StatusCode = 204;
                    response.Close();
                    return;
                case ErrorResult errorRes:
                    response.StatusCode = errorRes.StatusCode;
                    text = $"<h1>{errorRes.StatusCode} Error</h1><p>{errorRes.Message}</p>";
                    break;
                default:
                    response.StatusCode = 200;
                    text = result?.ToString() ?? "";
                    break;
            }

            buffer = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.OutputStream.Close();
        }

        private static Dictionary<string, string> ParseFormData(string body)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split('=', 2);
                result[Uri.UnescapeDataString(kv[0].Replace('+', ' '))] =
                    kv.Length > 1 ? Uri.UnescapeDataString(kv[1].Replace('+', ' ')) : "";
            }
            return result;
        }
    }
}