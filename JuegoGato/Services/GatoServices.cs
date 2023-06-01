using JuegoGato.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace JuegoGato.Services
{
    public class GatoServices
    {
        HttpClient client;

        public GatoServices()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://gato.sistemas19.com/");
            
        }

        public async Task<List<Jugador>> GetAll()
        {
            List<Jugador> jugadores = new List<Jugador>();
            var result = await client.GetAsync("api/Gato");
            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                jugadores = JsonConvert.DeserializeObject<List<Jugador>>(json);
            }
            return jugadores;
        }

        public async Task<Jugador> GetByName(string nombre)
        {
            Jugador j = new Jugador();
            var result = await client.GetAsync("api/Gato/"+nombre);
            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                j = JsonConvert.DeserializeObject<Jugador>(json);
                return j;
            }
            else
            {
                return null;
            }
           
        }

        public async Task<bool> Insert(Jugador j)
        {
            var json = JsonConvert.SerializeObject(j);
            StringContent scontent = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await client.PostAsync("api/Gato", scontent);
            if (!result.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> Update(Jugador j)
        {
            var json = JsonConvert.SerializeObject(j);
            StringContent scontent = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await client.PutAsync("api/Gato", scontent);
            if (!result.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
    }
}
