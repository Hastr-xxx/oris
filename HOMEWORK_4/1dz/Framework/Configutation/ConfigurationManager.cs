using System.IO;
using System.Text.Json;

namespace CustomHttpServer.Framework.Configuration
{
    public static class ConfigurationManager
    {
        public static ServerSettings Load()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл settings.json не найден!");

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ServerSettings>(json) ?? new ServerSettings();
        }
    }
}