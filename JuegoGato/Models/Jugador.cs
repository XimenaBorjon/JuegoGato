using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegoGato.Models
{
    public class Jugador
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public int PartidasGanadas { get; set; }
    }
}