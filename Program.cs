using System;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;

namespace FloydBotWeather;

class Program
{
    private static readonly string API_KEY = "05f7650ce0cd4981b71204408230704";
    private static string url = $"http://api.weatherapi.com/v1/current.json?key={API_KEY}";
    private static TelegramBotClient botClient;

    public static void Main(String[] args)
    {

        botClient = new TelegramBotClient("6259787206:AAH5aPj4sKJTRm91REOQwOKOzQVLqVD1wLM");

        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token);
        
        var me = botClient.GetMeAsync().Result;
        Console.WriteLine($"Empezando a escuchar {me.Username}");
        Console.WriteLine("Pulsa ENTER para parar de escuchar...");
        Console.ReadLine();
        cts.Cancel();

    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        // Si update.Message no es nulo, se almacena el resultado en la variable mensaje

        int cont = 0;

        if (update.Message is not { } mensaje)
            return;
        if (mensaje.Text is not { } mensajeText)
            return;

        var chatId = mensaje.Chat.Id;
        Console.WriteLine($"Recibido '{mensajeText}' en el chat #{chatId}");

        string respuesta;
        if (mensajeText == "/start")
            respuesta = $"Hola {mensaje.From.FirstName} soy Floyd y estoy aquí para ayudarte.\n" +
                        $"Dime una zona y te diré qué tiempo hace ahí.\n" +
                        $"Puedes usar el comando /floydzonelist para saber qué zonas son las que conozco :)";
        else if (mensajeText.StartsWith("Hola"))
            respuesta = $"¡Hola {mensaje.From.Username}!";
        //Consultar zonas de la API
        else if (mensajeText.Equals("/floydzonelist"))
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            // Verifica si la respuesta es un objeto JSON válido
            //if (result.StartsWith("{") && result.EndsWith("}"))
            //{
            //    respuesta = "Lo siento, no se pudo obtener la lista de ciudades.";
            //}
            //else
            //{
                List<Ciudad> ciudades = JsonConvert.DeserializeObject<List<Ciudad>>(result);
                respuesta = "Estas son las posibles ciudades:\n";

                foreach (Ciudad c in ciudades)
                {
                    respuesta += $"{cont++}." + c.ToString() + "\n";
                }
            //}
        }
        else
            respuesta = "Lo siento, no reconocí ese comando. Por favor, usa /start para iniciar o /help para ver la lista de comandos disponibles.";

        var enviarMensaje = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: respuesta,
            cancellationToken: ct);

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