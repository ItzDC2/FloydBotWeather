using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using System.Reflection;
using Telegram.Bot.Types.ReplyMarkups;

namespace FloydBotWeather;

class Program
{
    public static readonly string API_KEY = "589858ce33ff4dd2b6685857231404";
    private static TelegramBotClient botClient;
    private static bool respondiendo = false;
    private static string? opcionSeleccionada = null;
    private static readonly string CALL_FORMAT = "\nMe sería muy útil que me especificaras la ubicación de esta manera 👇\n" +
        "San Cristóbal de La Laguna, Islas Canarias, junto al código del país, por ejemplo España (ES) 📌";
    private static InlineKeyboardMarkup keyboard;

    private static long ResponseChatId = 0;
    private static List<long> ResponseChatIDs = new List<long>();

    public static void Main(String[] args)
    {

        botClient = new TelegramBotClient("6259787206:AAH5aPj4sKJTRm91REOQwOKOzQVLqVD1wLM");

        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        keyboard = new InlineKeyboardMarkup(new[]
                     {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Saber clima ⛅", "1"),
                        InlineKeyboardButton.WithCallbackData("Saber latitud y longitud 📍", "2")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Saber detalles del viento 🌀", "3"),
                        InlineKeyboardButton.WithCallbackData("Saber hora de la región 🕐", "4")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Saber calidad del aire 🍃", "5")
                    },
                    new []
                    {
                        InlineKeyboradButton.WithCallbackData("Parar la conversación ❌")
                    }

                });

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token); 
       
        Console.WriteLine("Pulsa ENTER para parar de escuchar...");
        Console.ReadLine();
        cts.Cancel();

    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        // Si update.Message no es nulo, se almacena el resultado en la variable mensaje

        int cont = 0;
        string respuesta = "";

        if (update.CallbackQuery != null && update.Message is null)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            if (!ResponseChatIDs.Contains(chatId))
                ResponseChatIDs.Add(chatId);
            switch (update.CallbackQuery.Data)
            {
                //Saber clima
                case "1":
                    respuesta = $"Dime la ubicación de la que quieres saber el clima 🌤{CALL_FORMAT}";
                    await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, null, ct);
                    opcionSeleccionada = "1";
                    respondiendo = true;
                    respuesta = "";
                    break;
                //Saber lat y long
                case "2":
                    respuesta = $"Dime la ubicación de la que quieres saber su latitud y longitud 📌{CALL_FORMAT}";
                    opcionSeleccionada= "2";
                    respondiendo = true;
                    await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, null, ct);
                    respuesta = "";
                    break;
                //Saber velocidad del viento
                case "3":
                    respuesta = $"Dime la ubicación de la que quieres detalles del viento allí 🌬{CALL_FORMAT}";
                    await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, null, ct);
                    opcionSeleccionada = "3";
                    respondiendo = true;
                    respuesta = "";
                    break;
                //Saber hora
                case "4":
                    respuesta = $"Dime la ubicación de la que quieres saber la hora que hace allí 🕐{CALL_FORMAT}";
                    await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, null, ct);
                    opcionSeleccionada = "4";
                    respondiendo = true;
                    respuesta = "";
                    break;
                //Saber calidad del aire
                case "5":
                    respuesta = $"Dime la ubicación de la quieres saber la calidad del aire 🌳{CALL_FORMAT}";
                    await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, null, ct);
                    opcionSeleccionada = "5";
                    respondiendo = true;
                    respuesta = "";
                    break;
                case "5":
                    respuesta = $"¡Muchas gracias por haber usado FloydBotWeather!\nSi necesitas algo más, ¡no dudes en preguntar!";    
                    await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, null, ct);
            }
        } else {
            var mensaje = update.Message;
            var mensajeText = mensaje?.Text;

            if (mensaje == null || mensajeText == null || mensajeText.Length == 0 || mensajeText == null)
                return;

            var chatId = mensaje.Chat.Id;
            Console.WriteLine($"Recibido '{mensajeText}' en el chat #{chatId}");

            if (respondiendo && ResponseChatIDs.Contains(chatId))
            {
                ResponseChatIDs.Remove(chatId);
                switch (opcionSeleccionada)
                {
                    //Saber clima
                    case "1":
                        respuesta = await ApiHandler.GetWeather(mensajeText, ct);
                        await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, keyboard, ct);
                        opcionSeleccionada = null;
                        respondiendo = false;
                        respuesta = "";
                        break;
                    //Saber lat y long
                    case "2":
                        respuesta = await ApiHandler.GetLatitudAndLongitude(mensajeText, ct);
                        await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, keyboard, ct);
                        opcionSeleccionada = null;
                        respondiendo = false;
                        respuesta = "";
                        break;
                    //Saber información del viento
                    case "3":
                        respuesta = await ApiHandler.GetWindDetails(mensajeText, ct);
                        await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, keyboard, ct);
                        opcionSeleccionada = null;
                        respondiendo = false;
                        respuesta = "";
                        break;
                    //Saber hora
                    case "4":
                        respuesta = await ApiHandler.GetTime(mensajeText, ct);
                        await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, keyboard, ct);
                        opcionSeleccionada = null;
                        respondiendo = false;
                        respuesta = "";
                        break;
                    //Saber calidad del aire
                    case "5":
                        respuesta = await ApiHandler.GetAirQuality(mensajeText, ct);
                        await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, keyboard, ct);
                        opcionSeleccionada = null;
                        respondiendo = false;
                        respuesta = "";
                        break;
                }
                respondiendo = false;
                opcionSeleccionada = null;
            } else {
                if (mensajeText == "/start")
                {
                    var saludo = $"Hola {mensaje.From.FirstName} 😊, soy Floyd y estoy aquí para ayudarte 😎\n" +
                                $"Elige lo que quieras que haga y con gusto lo haré 😁\n";

                    await ApiHandler.EnviarMensaje($"{saludo}Aquí debajo te dejo las opciones disponibles 👇", botClient, chatId, keyboard, ct);
                }
                else if (mensajeText.Equals("/dox"))
                {
                    string videoPath = "FloydBotWeather.Resources.doxed.mp4";
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(videoPath))
                    {
                        var video = new InputOnlineFile(stream, videoPath);
                        await botClient.SendVideoAsync(chatId, video, duration: 12, thumb: null, caption: ":)");
                    }
                    respuesta = "";
                }
                else
                {
                    await ApiHandler.EnviarMensaje($"No te he entendido, por favor, elige una de las opciones de aquí abajo 👇", botClient, chatId, keyboard, ct);
                    respuesta = "";
                }
            }

            if (respuesta != "")
                await ApiHandler.EnviarMensaje(respuesta, botClient, chatId, keyboard, ct);

        }

    }

    private async static Task<Task> HandlePollingErrorAsync(ITelegramBotClient botClient, Exception ex, CancellationToken ct)
    {
        var ErrorMsg = ex switch
        {
            ApiRequestException apiRequest => $"Telegram API Error:\n[{apiRequest.ErrorCode}]\n{apiRequest.Message}",
            _ => ex.ToString()
        };

        Console.WriteLine(ErrorMsg);
        return Task.CompletedTask;

    }

}