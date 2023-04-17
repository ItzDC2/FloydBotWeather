using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FloydBotWeather
{
class ApiHandler
    {

        private static readonly string DESPEDIDA = "\n¿Puedo ayudarte en algo más? 👇";
        public static async Task EnviarMensaje(string text, ITelegramBotClient bot, ChatId chatId, IReplyMarkup? reply, CancellationToken ct)
        {
            await bot.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    replyMarkup: (reply ?? null),
                    cancellationToken: ct
                );
        }
        public static async Task<string> GetWeather(string zone, CancellationToken ct)
        {
            var url = $"http://api.weatherapi.com/v1/current.json?key={Program.API_KEY}&q={zone}&lang=es";
            var response = "";

            try
            {

                var httpClient = new HttpClient();
                var api_response = await httpClient.GetAsync(url, ct);
                if(api_response.IsSuccessStatusCode)
                {
                    var responseString = await api_response.Content.ReadAsStringAsync();
                    dynamic data  = JsonConvert.DeserializeObject<dynamic>(responseString);
                    if(data != null)
                    {
                        string weather = data.current.condition.text;
                        double temp = data.current.temp_c;
                        double term = data.current.feelslike_c;
                        response = $"En {zone} ahora mismo está {weather.ToLower()} y hacen {temp}ºC" +
                            ((temp != term) ? $" aunque la sensación térmica es de {term}ºC." : ".")
                            + DESPEDIDA;
                        Console.WriteLine($"Buscado {zone}:\n * País: {data.location.country}\n * Región: {data.location.region}\n" +
                            $" * Nombre: {data.location.name}\n * Código: {data.location.tz_id}");
                    }

                }
            }
            catch (HttpRequestException ex)
            {
                response = $"No se ha podido encontrar información del tiempo para {zone}. Por favor, asegúrate de escribir bien el nombre de la ciudad.";
                Console.WriteLine(ex.StackTrace.ToString());
            }
            return response;
        }

        public static async Task<string> GetLatitudAndLongitude(string zone, CancellationToken ct)
        {
            var url = $"http://api.weatherapi.com/v1/current.json?key={Program.API_KEY}&q={zone}&lang=es";
            string response = "";
            try
            {
                var httpClient = new HttpClient();
                var api_response = await httpClient.GetAsync(url, ct);
                if (api_response.IsSuccessStatusCode)
                {
                    string responseString = await api_response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(responseString);
                    double lat = data.location.lat;
                    double lon = data.location.lon;
                    response = $"Las coordenadas de {zone} son:\n* Latitud: {lat}º\n* Longitud: {lon}º" +
                        $"{DESPEDIDA}";
                }
                else
                    response = $"No se ha podido encontrar información de la zona {zone}, ¿Has escrito bien su nombre?";
            
            } catch (HttpRequestException ex)
            {
                response = $"No se ha podido encontrar información de la zona {zone}, ¿Has escrito bien su nombre?";
                Console.WriteLine(ex.StackTrace.ToString());
            }
            return response;
        }

    }
}
