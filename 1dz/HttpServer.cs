using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class HttpServer
{
    private readonly HttpListener _listener = new HttpListener();
    private CancellationTokenSource _cts;

    public void Start()
    {
        try
        {
            string jsonString = File.ReadAllText("settings.json");
            var settings = JsonSerializer.Deserialize<ServerSettings>(jsonString);

            if (settings?.Prefixes == null || settings.Prefixes.Length == 0)
            {
                Console.WriteLine("Ошибка: В settings.json не указаны префиксы.");
                return;
            }

            foreach (var prefix in settings.Prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            _listener.Start();
            _cts = new CancellationTokenSource();

            Task.Run(() => ListenLoopAsync(_cts.Token));

            Console.WriteLine("Сервер успешно запущен. Ожидание подключений...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при запуске сервера: {ex.Message}");
        }
    }

    private async Task ListenLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();

                _ = Task.Run(() => ProcessRequestAsync(context));
            }
            catch (HttpListenerException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;
        string path = "www/index.html"; 

        if (!File.Exists(path))
        {
            response.StatusCode = 404;
            byte[] err = Encoding.UTF8.GetBytes("<h1>404 — файл не найден</h1>");
            response.ContentLength64 = err.Length;
            await response.OutputStream.WriteAsync(err, 0, err.Length);
            response.Close();
            return;
        }

        string responseString = await File.ReadAllTextAsync(path, Encoding.UTF8);

        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html; charset=utf-8";

        try
        {
            using (Stream output = response.OutputStream)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
                await output.FlushAsync();
            }
            Console.WriteLine("Запрос обработан");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке ответа: {ex.Message}");
        }
        finally
        {
            response.Close();
        }
    }

    public void Stop()
    {
        if (_listener.IsListening)
        {
            _cts?.Cancel(); 
            _listener.Stop(); 
            _listener.Close(); 
            Console.WriteLine("Сервер корректно остановлен.");
        }
    }
}