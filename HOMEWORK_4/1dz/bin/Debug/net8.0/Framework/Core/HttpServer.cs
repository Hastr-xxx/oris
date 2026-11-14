using System;
using System.IO;
using System.Net;
using CustomHttpServer.Framework.Handlers;

namespace CustomHttpServer.Framework
{
    public class HttpServer
    {
        private readonly HttpListener _listener = new();
        private readonly RequestHandler _rootHandler;
        private bool _isRunning;

        public HttpServer()
        {
            string staticRoot = Path.Combine(Directory.GetCurrentDirectory(), "static");

            var staticFilesHandler = new StaticFilesHandler(staticRoot);
            var notFoundHandler = new NotFoundHandler();

            staticFilesHandler.Successor = notFoundHandler;
            _rootHandler = staticFilesHandler;
        }

        public void Start(string prefix)
        {
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"Сервер запущен на {prefix}");
            Receive();
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер завершил свою работу");
        }

        private void Receive()
        {
            if (_isRunning && _listener.IsListening)
                _listener.BeginGetContext(ListenerCallback, _listener);
        }

        private async void ListenerCallback(IAsyncResult result)
        {
            if (!_isRunning) return;

            HttpListenerContext context;
            try { context = _listener.EndGetContext(result); }
            catch { return; }

            try
            {
                await _rootHandler.HandleRequestAsync(context);
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