using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static SmartHome.Program;

namespace SmartHome
{
    public class UpdateHandler : IUpdateHandler
    {
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            var errorMessage = exception.ToString();
            Console.WriteLine(errorMessage);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await HandleMessageAsync(update.Message!, cancellationToken);
                        break;

                    case UpdateType.CallbackQuery:
                        await HandleCallbackQueryAsync(botClient, update.CallbackQuery!, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, HandleErrorSource.PollingError, cancellationToken);
            }
        }

        private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (string.Equals(message.Text, "/start", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(message.Text, "/menu", StringComparison.OrdinalIgnoreCase))
            {
                await SendMainMenu(message.Chat.Id, cancellationToken);
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            try
            {
                if (callbackQuery.Message?.Chat == null)
                {
                    await botClient.AnswerCallbackQuery(
                        callbackQueryId: callbackQuery.Id,
                        text: "Ошибка: не удалось получить информацию о чате",
                        cancellationToken: cancellationToken);
                    return;
                }

                var chatId = callbackQuery.Message.Chat.Id;

                if (callbackQuery.From?.Id == null)
                {
                    Console.WriteLine("Не удалось получить UserID");
                    await botClient.AnswerCallbackQuery(
                        callbackQueryId: callbackQuery.Id,
                        text: "Ошибка: Не удалось идентифицировать пользователя",
                        cancellationToken: cancellationToken);
                    return;
                }

                var userId = callbackQuery.From.Id;

                switch (callbackQuery.Data)
                {
                    case "ac_power_toggle":
                        var states = await GetDeviceStates();
                        var newPowerState = !states.AcIsOn;

                        await SetPowerState(AcDevideId, newPowerState, userId);

                        await SendMainMenu(chatId, cancellationToken);

                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            text: $"Кондиционер {(newPowerState ? "включен" : "выключен")}!",
                            cancellationToken: cancellationToken);
                        break;

                    case "light_power_toggle":
                        states = await GetDeviceStates();
                        var newPowerLightState = !states.LightIsOn;

                        await SetPowerState(LightDeviceId, newPowerLightState, userId);

                        await SendMainMenu(chatId, cancellationToken);

                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            text: $"Свет {(newPowerLightState ? "включен" : "выключен")}!",
                            cancellationToken: cancellationToken);
                        break;

                    case "ac_temp_menu":
                        await SendTemperatureMenu(chatId, cancellationToken);
                        break;

                    case "light_temp_menu":
                        await SendLightMenu(chatId, cancellationToken);
                        break;

                    case "light_2700":
                    case "light_4000":
                    case "light_6500":
                        var lightTemperature = int.Parse(callbackQuery.Data.Split('_')[1]);
                        await SetLightTemperature(lightTemperature, LightDeviceId, userId);

                        await SendMainMenu(callbackQuery.Message.Chat.Id, cancellationToken);

                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            text: $"Оттенок света изменен на {lightTemperature}K",
                            cancellationToken: cancellationToken
                            );
                        break;

                    case "temp_18":
                    case "temp_20":
                    case "temp_22":
                    case "temp_24":
                    case "temp_26":
                    case "temp_28":
                        var temp = int.Parse(callbackQuery.Data.Split('_')[1]);
                        await SetTemperature(temp, LightDeviceId, userId);

                        await SendMainMenu(callbackQuery.Message.Chat.Id, cancellationToken);

                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            text: $"Температура установлена на {temp} °C!",
                            cancellationToken: cancellationToken
                            );
                        break;


                    case "menu":
                        await SendMainMenu(callbackQuery.Message.Chat.Id, cancellationToken);
                        break;
                }

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task SendMainMenu(long chatId, CancellationToken cancellationToken)
        {
            // Всегда получаем свежее состояние
            var states = await GetDeviceStates();

            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"Кондиционер: {(states.AcIsOn ? "Выкл" : "Вкл")}",
                    "ac_power_toggle"),
                    InlineKeyboardButton.WithCallbackData("Установить температуру", "ac_temp_menu")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                     $"Свет: {(states.LightIsOn ? "Выкл" : "Вкл")}",
                     "light_power_toggle"),
                    InlineKeyboardButton.WithCallbackData("Оттенок света", "light_temp_menu")
                }
            });

            await botClient.SendMessage(
                chatId,
                $"{(states.AcIsOn ? "Кондиционер включен" : "Кондиционер выключен")}\n" +
                $"Целевая температура: {states.AcTemperature}°C\n" +
                $"Температура в комнате: {states.CurrentTemperature}°C\n\n" +
                $"{(states.LightIsOn ? "Свет включен" : "Свет выключен")}\n" +
                $"Оттенок света: {states.LightTemperature}\n\n" +
                $"Обновлено: {DateTime.Now:HH:mm:ss}",
                replyMarkup: markup,
                cancellationToken: cancellationToken
            );
        }

        private static async Task SendTemperatureMenu(long chatId, CancellationToken cancellationToken)
        {
            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("18°C", "temp_18"),
                    InlineKeyboardButton.WithCallbackData("20°C", "temp_20"),
                    InlineKeyboardButton.WithCallbackData("22°C", "temp_22")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("24°C", "temp_24"),
                    InlineKeyboardButton.WithCallbackData("26°C", "temp_26"),
                    InlineKeyboardButton.WithCallbackData("28°C", "temp_28")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "menu")
                }
            });

            await botClient.SendMessage(
            chatId,
            "Выберите температуру:",
                replyMarkup: markup,
                cancellationToken: cancellationToken
            );
        }

        private static async Task SendLightMenu(long chatId, CancellationToken cancellationToken)
        {
            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Теплый (2700K)", "light_2700"),
                    InlineKeyboardButton.WithCallbackData("Нейтральный (4000K)", "light_4000")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Холодный (6500K)", "light_6500"),
                    InlineKeyboardButton.WithCallbackData("Назад", "menu")
                }
            });

            await botClient.SendMessage(
            chatId,
            "Выберите оттенок света:",
                replyMarkup: markup,
                cancellationToken: cancellationToken
            );
        }

        private static async Task<DeviceStates> GetDeviceStates()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", YandexToken);


            var response = await _httpClient.GetAsync("https://api.iot.yandex.net/v1.0/user/info");
            var content = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<YandexDeviceListResponse>(content);

            var states = new DeviceStates();

            foreach (var device in responseData.Devices)
            {
                if (device.Type == "devices.types.thermostat.ac")
                {
                    // Состояние кондиционера
                    var onOffCapability = device.Capabilities.FirstOrDefault(c =>
                        c.Type == "devices.capabilities.on_off");
                    var tempCapability = device.Capabilities.FirstOrDefault(c =>
                        c.Type == "devices.capabilities.range" && c.State.Instance == "temperature");

                    // Текущая температура из properties
                    var tempProperty = device.Properties.FirstOrDefault(p =>
                        p.Type == "devices.properties.float" && p.State.Instance == "temperature");

                    states.AcIsOn = onOffCapability != null && Convert.ToBoolean(onOffCapability.State.Value);
                    states.AcTemperature = tempCapability != null ? Convert.ToInt32(tempCapability.State.Value) : 0;
                    states.CurrentTemperature = tempProperty != null ? Convert.ToInt32(tempProperty.State.Value) : 0;
                }
                else if (device.Type == "devices.types.light")
                {
                    // Состояние света
                    var onOffCapability = device.Capabilities.FirstOrDefault(c =>
                        c.Type == "devices.capabilities.on_off");
                    var colorCapability = device.Capabilities.FirstOrDefault(c =>
                        c.Type == "devices.capabilities.color_setting");

                    states.LightIsOn = onOffCapability != null && Convert.ToBoolean(onOffCapability.State.Value);
                    states.LightTemperature = colorCapability != null ? Convert.ToInt32(colorCapability.State.Value) : 0;
                }
            }

            return states;
        }

        private static async Task SetLightTemperature(int temperature, string deviceId, long userId)
        {
            var payload = new
            {
                devices = new[]
                {
                    new
                    {
                        id = LightDeviceId,
                        actions = new[]
                            {
                                new
                                {
                                    type = "devices.capabilities.color_setting",
                                    state = new
                                    {
                                        instance = "temperature_k",
                                        value = Math.Clamp(temperature, 2000, 6500)
                                    }
                                }
                        }
                    }
                }
            };
            await SendDeviceCommand(payload);

            await _logService.LogActionAsync(userId, deviceId, "SET_LIGHT_TEMPERATURE",
            $"{{\"light_temperature\":{temperature}}}");
        }

        private static async Task SetPowerState(string deviceId, bool enabled, long userId)
        {
            var payload = new
            {
                devices = new[]
                {
                    new
                    {
                        id = deviceId,
                        actions = new[]
                        {
                            new
                            {
                                type = "devices.capabilities.on_off",
                                state = new { instance = "on", value = enabled }
                            }
                        }
                    }
                }
            };

            await SendDeviceCommand(payload);

            await _logService.LogActionAsync(userId, deviceId, "POWER_TOGGLE",
            $"{{\"state\":{enabled.ToString().ToLower()}}}");
        }

        private static async Task SetTemperature(int temperature, string deviceId, long userId)
        {
            var payload = new
            {
                devices = new[]
                {
                    new
                    {
                        id = AcDevideId,
                        actions = new[]
                        {
                            new
                            {
                                type = "devices.capabilities.range",
                                state = new
                                {
                                    instance = "temperature",
                                    value = temperature
                                }
                            }
                        }
                    }
                }
            };

            await SendDeviceCommand(payload);

            await _logService.LogActionAsync(userId, deviceId, "SET_TEMPERATURE",
            $"{{\"temperature\":{temperature}}}");
        }

        private static async Task SendDeviceCommand(object payload)
        {
            var content = new StringContent(
            JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", YandexToken);

            var response = await _httpClient.PostAsync(
                "https://api.iot.yandex.net/v1.0/devices/actions",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка API: {response.StatusCode} - {errorContent}");
            }
        }
    }
}
