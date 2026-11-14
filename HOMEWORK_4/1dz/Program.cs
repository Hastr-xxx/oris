using System;
using CustomHttpServer.Framework;
using CustomHttpServer.Framework.Configuration;

class Program
{
    static void Main(string[] args)
    {
        var settings = ConfigurationManager.Load();
        string prefix = settings.Prefixes.Length > 0 ? settings.Prefixes[0] : "http://127.0.0.1:8888/";

        HttpServer server = new HttpServer();
        server.Start(prefix);

        Console.WriteLine("Введите 'stop' для завершения работы сервера.");

        while (true)
        {
            string command = Console.ReadLine();
            if (command?.ToLower().Trim() == "stop")
            {
                server.Stop();
                break;
            }
        }
    }
}