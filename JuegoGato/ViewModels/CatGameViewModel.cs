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
using JuegoGato.Services;
using Microsoft.Maui.Layouts;

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

        GatoServices services = new GatoServices();

        private bool noganadores;
        private bool iniciado=false;

        private byte playerTurn = 0;
        private int turno = 0;

        public Command RegistrarCommand { get; set; }
        public Command LoginCommand { get; set; }
        public Command IniciarCommand { get; set; }
        public Command VerSalasCommand { get; set; }

        Jugador myPlayer = new();
        List<int[]> PosiblesGanadores = new List<int[]>();
        public HubConnection connection = new HubConnectionBuilder().WithUrl("https://gato.sistemas19.com/gatohub").Build();

        public ObservableCollection<CatModel> CatsList { get; set; } = new ObservableCollection<CatModel>();
        public ObservableCollection<Jugador> ListJugador { get; set; } = new ObservableCollection<Jugador>();
        public Jugador jugadores{get; set;} = new Jugador();

        public CatGameViewModel()
        {
            RegistrarCommand = new Command(RegistrarJugador);
            LoginCommand = new Command<string>(LoginJugador);
            IniciarCommand = new Command(Iniciar);
            VerSalasCommand = new Command(VerScore);
            SetUpGameInfo();
            connection.StartAsync();
            connection.On("SalaLlena", async () =>
            {
                await Shell.Current.DisplayAlert("Error al unirse", "La sala está llena", "OK");
            });
            connection.On("SetTurn", () =>
            {
                playerTurn = 2;
            });
            RegisterMoveHandler();

        }

        private void Iniciar()
        {
            
        }

        public async void LoginJugador(string jugadore)
        {
            myPlayer = await services.GetByName(jugadore);

            if (myPlayer != null)
            {
                //await connection.InvokeAsync("Join");
                await Shell.Current.GoToAsync("//Juego");
            }
        }

        public async void VerScore()
        {
            ListJugador.Clear();
            var datos = await services.GetAll();
            datos.ForEach(x=>ListJugador.Add(x));
            await Shell.Current.GoToAsync("//Score");
        }

        public async void RegistrarJugador()
        {
            ListJugador.Add(jugadores);
            await services.Insert(jugadores);
            await App.Current.MainPage.DisplayAlert("Registrado", "Te has registrado exitosamente", "Ok");
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
            if (turno == 0)
            {
                playerTurn = 1;
                await connection.InvokeAsync("SendFirstMove", selectedItem.Index, selectedItem.SelectedText, connection.ConnectionId);
            }
            else
            {
                await connection.InvokeAsync("SendMove", selectedItem.Index, selectedItem.SelectedText, playerTurn);
            }
            
        }
        //Movimiento del otro jugador "Recivir"
        private void RegisterMoveHandler()
        {
            connection.On<int, string, byte>("ReceiveMove", (index, selectedText, turn) =>
             {
                var selectedCat = CatsList.FirstOrDefault(cat => cat.Index == index);
                if (selectedCat != null)
                {
                     turno++;
                     selectedCat.SelectedText = selectedText;
                    selectedCat.Player = turnojugador == 0 ? 1 : 0;
                    turnojugador = selectedCat.Player.Value;
                    VerGanador(playerTurn, myPlayer);

                }
            });

        }


        async Task SetUpGameInfo()
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
        public async Task ReinicarJuego()
        {
            playerTurn = 0;
            turno = 0;
            noganadores = false;
            PlayerWinOrDrawText = "";
            turnojugador = 0;
            await SetUpGameInfo();
        }


        [ICommand]
        public async void SelectedItem(CatModel selectditem)
        {
            if((turno%2==0 && playerTurn==1) || (turno % 2 != 0 && playerTurn == 2) ||(turno==0 && playerTurn==0))
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
                await SendMoveToserver(selectditem);
            }
            //if (turno == 0)
            //{
            //    await SendMoveToserver(selectditem);
            //}
        }

        private async void VerGanador(byte turnn, Jugador a)
        {
            var player1IndexList = CatsList.Where(x => x.Player == 0).Select(x => x.Index).ToList();
            var player2IndexList = CatsList.Where(x => x.Player == 1).Select(x => x.Index).ToList();
            byte ganador = 0;
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
                            ganador = 1;
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
                            ganador = 2;
                            noganadores = true;
                            
                            break;
                        }
                    }

                }
                if (ganador != 0)
                {
                    
                    
                        if (ganador != turnn)
                        {
                            if (playerTurn != 0)
                            {
                                await ReinicarJuego();
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await Shell.Current.DisplayAlert("Ganaste", "Jugador " + ganador + " Ganador", "OK");
                            });
                            await services.Update(myPlayer);

                            }
                        }
                        else
                        {
                            if (playerTurn != 0)
                            {
                                await ReinicarJuego();
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await Shell.Current.DisplayAlert("Perdiste", "Jugador " + ganador + " Ganador", "OK");
                            });
                            }
                        }
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await Shell.Current.GoToAsync("..");
                        });
                    }
            }

            if (CatsList.Count(f=>f.Player.HasValue) == 9 && !noganadores)
            {   
                PlayerWinOrDrawText = "Empate";
            }
        }
    }
}
