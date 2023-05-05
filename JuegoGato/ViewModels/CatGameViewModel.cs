using CommunityToolkit.Mvvm.ComponentModel;
using JuegoGato.Models;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace JuegoGato.ViewModels
{
    public partial class CatGameViewModel : ObservableObject
    {
        private HubConnection _hubConnection;
        [ObservableProperty]
        private int _jugador1puntos;

        [ObservableProperty]
        private int _jugador2puntos;

        [ObservableProperty]
        private string _playerWinOrDrawText;

        [ObservableProperty]
        private int turnojugador = 0;


        private bool noganadores;

        List<int[]> PosiblesGanadores = new List<int[]>();
        public HubConnection connection = new HubConnectionBuilder().WithUrl("https://gato.sistemas19.com/gatohub").Build();

        public ObservableCollection<CatModel> CatsList { get; set; } = new ObservableCollection<CatModel>();
        public CatGameViewModel()
        {
            SetUpGameInfo();
            connection.StartAsync();
            //llamar a los metodos

            RegisterMoveHandler();
        }

        public async Task ConnectToserver()
        {
            await _hubConnection.StartAsync();
        }
        //desconectar al servidor
        public async Task DisconnectFromServer()
        {
            await _hubConnection.StopAsync();
        }
        //Movimiento de jugador al signalR
        public async Task SendMoveToserver(CatModel selectedItem)
        {
            if(connection.State==HubConnectionState.Disconnected) 
            {
                await connection.StartAsync();
            }
            await connection.InvokeAsync("SendMove", selectedItem.Index, selectedItem.SelectedText);
        }
        //Movimiento del otro jugador "Recivir"
        private void RegisterMoveHandler()
        {
            connection.On<int, string>("ReceiveMove", async (index, selectedText) =>
             {
                var selectedCat = CatsList.FirstOrDefault(cat => cat.Index == index);
                if (selectedCat != null)
                {
                    selectedCat.SelectedText = selectedText;
                    selectedCat.Player = turnojugador == 0 ? 1 : 0;
                    turnojugador = selectedCat.Player.Value;
                    VerGanador();

                }
            });
        }


        private void SetUpGameInfo()
        {
            PosiblesGanadores.Clear();

            PosiblesGanadores.Add(new[] { 0, 1, 2 });
            PosiblesGanadores.Add(new[] { 3, 4, 5 });
            PosiblesGanadores.Add(new[] { 6, 7, 8 });

            PosiblesGanadores.Add(new[] { 0, 3, 6 });
            PosiblesGanadores.Add(new[] { 1, 4, 7 });
            PosiblesGanadores.Add(new[] { 2, 5, 8 });

            PosiblesGanadores.Add(new[] { 0, 4, 8 });
            PosiblesGanadores.Add(new[] { 2, 4, 6 });


            CatsList.Clear();
            CatsList.Add(new CatModel(0));
            CatsList.Add(new CatModel(1));
            CatsList.Add(new CatModel(2));
            CatsList.Add(new CatModel(3));
            CatsList.Add(new CatModel(4));
            CatsList.Add(new CatModel(5));
            CatsList.Add(new CatModel(6));
            CatsList.Add(new CatModel(7));
            CatsList.Add(new CatModel(8));
        }

        [ICommand]
        public void ReinicarJuego()
        {
            noganadores = false;
            PlayerWinOrDrawText = "";
            turnojugador = 0;
            SetUpGameInfo();
        }


        [ICommand]
        public async void SelectedItem(CatModel selectditem)
        {
            if (!string.IsNullOrWhiteSpace(selectditem.SelectedText) || noganadores) return;
            

            if (turnojugador == 0)
            {
                selectditem.SelectedText = "X"; //Jugador 1
            }
            else
            {
                selectditem.SelectedText = "O"; //Jugador 2
            }
            selectditem.Player = turnojugador;
            //turnojugador = turnojugador == 0 ? 1 : 0;
            //VerGanador();
            await SendMoveToserver(selectditem);
        }

        private void VerGanador()
        {
            var player1IndexList = CatsList.Where(x => x.Player == 0).Select(x => x.Index).ToList();
            var player2IndexList = CatsList.Where(x => x.Player == 1).Select(x => x.Index).ToList();

            if (player1IndexList.Count > 2 || player2IndexList.Count > 2)
            {
                foreach (var posiblesganadores in PosiblesGanadores)
                {
                    if (noganadores) break;
                    int player1Count = 0;
                    int player2Count = 0;

                    foreach (var index in player1IndexList)
                    {
                        if (posiblesganadores.Contains(index))
                        {
                            player1Count++;
                        }
                        if (player1Count == 3)
                        {
                            Jugador1puntos++;
                            PlayerWinOrDrawText = "Jugador 1 Ganador";
                            noganadores = true;
                            break;
                        }
                    }

                    foreach (var index in player2IndexList)
                    {
                        if (posiblesganadores.Contains(index))
                        {
                            player2Count++;
                        }
                        if (player2Count == 3)
                        {
                            Jugador2puntos++;
                            PlayerWinOrDrawText = "Jugador 2 Ganador";
                            noganadores = true;
                            break;
                        }
                    }
                }

            }

            if (CatsList.Count(f=>f.Player.HasValue) == 9 && !noganadores)
            {
                PlayerWinOrDrawText = "Empate";
            }
        }
    }
}
