using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        HttpServer server = new HttpServer();

        server.Start();

        Console.WriteLine("Введите 'stop' для завершения работы сервера.");

        while (true)
        {
            string command = Console.ReadLine();

            if (command?.ToLower().Trim() == "stop")
            {
                server.Stop();
                break;
            }
            else if (!string.IsNullOrWhiteSpace(command))
            {
                Console.WriteLine("Неизвестная команда. Введите 'stop' для выхода.");
            }
        }
    }
}