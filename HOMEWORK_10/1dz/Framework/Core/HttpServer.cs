using System;
using System.IO;
using System.Net;
using CustomHttpServer.Framework.Handlers;
using CustomHttpServer.Framework.Authentication;
using CustomHttpServer.Services.Authentication;

namespace CustomHttpServer.Core
{
    public class HttpServer
    {
        private readonly HttpListener listener = new();
        private readonly RequestHandler rootHandler;
        private readonly string staticRoot;
        private bool isRunning;


        public HttpServer()
        {
            staticRoot = Path.Combine(Directory.GetCurrentDirectory(), "static", "Login Form");

            IUserSessionService sessionService = new UserSessionService();

            var controllerHandler = new ControllersHandler(sessionService);
            var staticHandler = new StaticFilesHandler(staticRoot);
            controllerHandler.Successor = staticHandler;

            rootHandler = controllerHandler;
        }

        public void Start()
        {
            if (!Directory.Exists(staticRoot))
            {
                Console.WriteLine($"Ошибка: папка {staticRoot} не найдена!");
                return;
            }

            listener.Prefixes.Add("http://127.0.0.1:8888/");
            listener.Start();
            isRunning = true;

            Console.WriteLine($"Корень статики: {staticRoot}");
            Console.WriteLine("Сервер начал свою работу");
            Receive();
        }

        public void Stop()
        {
            if (!isRunning) return;
            isRunning = false;
            listener.Stop();
            listener.Close();
            Console.WriteLine("Сервер завершил свою работу");
        }

        private void Receive()
        {
            if (isRunning && listener.IsListening)
                listener.BeginGetContext(ListenerCallback, listener);
        }

        private async void ListenerCallback(IAsyncResult result)
        {
            if (!isRunning) return;

            HttpListenerContext context;
            try
            {
                context = listener.EndGetContext(result);
            }
            catch (HttpListenerException) { return; }
            catch (ObjectDisposedException) { return; }

            try
            {
                await rootHandler.HandleRequest(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
            }
            finally
            {
                Receive();
            }
        }
    }
}