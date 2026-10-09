using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CustomHttpServer.Framework.Attributes;

namespace CustomHttpServer.Framework.Handlers
{
    public class ControllerHandler : Handler
    {
        private readonly Dictionary<string, (Type Controller, MethodInfo Method)> _routes = new();

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

                    if (getAttr != null)
                        _routes[$"GET:/{controllerAttr.Route}/{getAttr.Route}".ToLowerInvariant()] = (type, method);

                    if (postAttr != null)
                        _routes[$"POST:/{controllerAttr.Route}/{postAttr.Route}".ToLowerInvariant()] = (type, method);
                }
            }

            Console.WriteLine($"[ControllerHandler] Зарегистрировано маршрутов: {_routes.Count}");
            foreach (var key in _routes.Keys)
                Console.WriteLine($"  {key}");
        }

        public override async Task HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Url?.LocalPath ?? "/";
            string method = request.HttpMethod.ToUpperInvariant();
            string key = $"{method}:{path}".ToLowerInvariant();

            if (_routes.TryGetValue(key, out var route))
            {
                await ExecuteAction(route.Controller, route.Method, context);
                return;
            }

            if (Successor != null)
                await Successor.HandleRequest(context);
            else
            {
                response.StatusCode = 404;
                response.OutputStream.Close();
            }
        }

        private async Task ExecuteAction(Type controllerType, MethodInfo method, HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            object controllerInstance = Activator.CreateInstance(controllerType)!;
            var parameters = method.GetParameters();
            object?[] args = new object?[parameters.Length];

            if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) && request.HasEntityBody)
            {
                string body;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    body = await reader.ReadToEndAsync();

                var form = ParseFormData(body);
                Console.WriteLine($"[ControllerHandler] POST body: {body}");

                for (int i = 0; i < parameters.Length; i++)
                {
                    string name = parameters[i].Name!;
                    if (form.TryGetValue(name, out var value))
                        args[i] = value;
                    else
                        args[i] = parameters[i].ParameterType.IsValueType
                            ? Activator.CreateInstance(parameters[i].ParameterType)
                            : null;
                }
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++)
                    args[i] = parameters[i].ParameterType.IsValueType
                        ? Activator.CreateInstance(parameters[i].ParameterType)
                        : null;
            }

            object? result = method.Invoke(controllerInstance, args);

            string text = result?.ToString() ?? "OK";
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            response.StatusCode = 200;
            response.ContentType = "text/plain; charset=utf-8";
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
                string key = Uri.UnescapeDataString(kv[0].Replace('+', ' '));
                string value = kv.Length > 1 ? Uri.UnescapeDataString(kv[1].Replace('+', ' ')) : "";
                result[key] = value;
            }

            return result;
        }
    }
}