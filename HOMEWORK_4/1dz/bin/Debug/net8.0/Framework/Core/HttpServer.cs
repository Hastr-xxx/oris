using System;
using System.IO;
using System.Net;
using CustomHttpServer.Framework.Handlers;

namespace CustomHttpServer.Core
{
    public class HttpServer
    {
        private readonly HttpListener _listener = new();
        private readonly Handler _rootHandler;
        private readonly string _staticRoot;
        private bool _isRunning;

        public HttpServer()
        {
            _staticRoot = Path.Combine(Directory.GetCurrentDirectory(), "static", "Login Form");

            var controllerHandler = new ControllerHandler();
            var staticHandler = new StaticFileHandler(_staticRoot);
            controllerHandler.Successor = staticHandler;

            _rootHandler = controllerHandler;
        }

        public void Start()
        {
            if (!Directory.Exists(_staticRoot))
            {
                Console.WriteLine($"Ошибка: папка {_staticRoot} не найдена!");
                return;
            }

            _listener.Prefixes.Add("http://127.0.0.1:8888/");
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"Корень статики: {_staticRoot}");
            Console.WriteLine("Сервер начал свою работу");
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
            try
            {
                context = _listener.EndGetContext(result);
            }
            catch (HttpListenerException) { return; }
            catch (ObjectDisposedException) { return; }

            try
            {
                await _rootHandler.HandleRequest(context);
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