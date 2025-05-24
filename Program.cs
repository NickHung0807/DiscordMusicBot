using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
class Program
{
    public async static Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
        string? botToken = config["BotToken"]?.ToString();
        if (string.IsNullOrEmpty(botToken))
        {
            Console.WriteLine("BotToken is not set in config.json");
            return;
        }
        var bot = new MusicBot(botToken);
        await bot.StartAsync();
    }
}