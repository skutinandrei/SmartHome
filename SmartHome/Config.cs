using DotNetEnv;

namespace SmartHome
{
    public static class Config
    {
        static Config()
        {
            Env.Load();
        }

        public static string YandexToken => Env.GetString("YANDEX_TOKEN");
        public static string AcDeviceId => Env.GetString("AC_DEVICE_ID");
        public static string LightDeviceId => Env.GetString("LIGHT_DEVICE_ID");
        public static string TelegramBotToken => Env.GetString("TELEGRAM_BOT_TOKEN");
    }
}
