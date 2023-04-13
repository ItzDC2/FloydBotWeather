using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloydBotWeather
{
    class Ciudad
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Region { get; set; }
        public string Pais { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Url { get; set; }
    
        public string ToString()
        {
            return $"Nombre: {Nombre}\n" +
                $"Región: {Region}\n" +
                $"País: {Pais}\n" +
                $"Latitud: {Lat}\n" +
                $"Longitud: {Lon}";
        }


    }

}
