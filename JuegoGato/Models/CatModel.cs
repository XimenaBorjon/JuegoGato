using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegoGato.Models
{
    public partial class CatModel : ObservableObject
    {
        public CatModel(int index) 
        {
            this.Index = index;
        }

        public int Index { get; set; }

        [ObservableProperty]
        private string selectedText;

        //Player 0 es X y Player 1 es O
        public int? Player { get; set; }
    }
}
