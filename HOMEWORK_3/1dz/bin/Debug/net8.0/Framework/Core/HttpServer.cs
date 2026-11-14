using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CustomHttpServer.Core;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly string _rootDirectory;
    private bool _isRunning;

    public HttpServer()
    {
        _rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "static");
    }

    public void Start()
    {
        if (!Directory.Exists(_rootDirectory))
        {
            Console.WriteLine($"Ошибка: папка {_rootDirectory} не найдена!");
            Console.WriteLine("Сервер не может быть запущен.");
            return;
        }

        _listener.Prefixes.Add("http://127.0.0.1:8888/");
        _listener.Start();
        _isRunning = true;

        Console.WriteLine("Сервер начал свою работу");
        Receive();
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

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
        if (!_isRunning)
            return;

        HttpListenerContext context;

        try
        {
            context = _listener.EndGetContext(result);
        }
        catch (HttpListenerException)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        var request = context.Request;
        var response = context.Response;

        try
        {
            string localPath = request.Url?.LocalPath ?? "/";
            string filePath = GetFilePath(localPath);

            if (!File.Exists(filePath))
            {
                response.StatusCode = 404;
                filePath = Path.Combine(_rootDirectory, "404.html");

                if (!File.Exists(filePath))
                {
                    response.ContentType = "text/plain; charset=utf-8";
                    byte[] notFoundBytes = Encoding.UTF8.GetBytes("404 Not Found");
                    response.ContentLength64 = notFoundBytes.Length;
                    await response.OutputStream.WriteAsync(notFoundBytes);
                    return;
                }
            }

            response.ContentType = ContentTypeHelper.GetContentType(filePath);

            byte[] buffer = await File.ReadAllBytesAsync(filePath);

            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");

            try
            {
                response.StatusCode = 500;
            }
            catch
            {
                // ответ уже мог быть начат
            }
        }
        finally
        {
            response.OutputStream.Close();
            Receive();
        }
    }

    private string GetFilePath(string localPath)
    {
        string relativePath = localPath.TrimStart('/');

        // Если запрошен корень "/" или папка, отдаём index.html
        if (string.IsNullOrWhiteSpace(relativePath) || localPath.EndsWith('/'))
        {
            relativePath = Path.Combine(relativePath, "index.html");
        }

        string fullPath = Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));
        string rootWithSeparator = Path.GetFullPath(_rootDirectory) + Path.DirectorySeparatorChar;

        // Защита от выхода за пределы static
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(_rootDirectory, "404.html");
        }

        return fullPath;
    }
}