using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace SmartHome
{
    public class Program
    {
        public static string YandexToken = Config.YandexToken;
        public static string AcDevideId = Config.AcDeviceId;
        public static string LightDeviceId = Config.LightDeviceId;

        public static HttpClient _httpClient = new HttpClient();

        public static LogService _logService = new LogService();

        public static TelegramBotClient botClient = new TelegramBotClient(Config.TelegramBotToken);

        public static async Task Main()
        {
            await _logService.EnsureDatabaseCreatedAsync();

            using var cancellationToken = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                DropPendingUpdates = true,
            };

            var me = await botClient.GetMe();

            var handler = new UpdateHandler();

            botClient.StartReceiving(
                updateHandler: handler,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken.Token
            );

            Console.WriteLine($"{me.FirstName} запущен!");
            await Task.Delay(-1);
        }
    }
}