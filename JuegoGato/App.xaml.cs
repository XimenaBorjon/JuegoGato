using JuegoGato.ViewModels;
using JuegoGato.Views;

namespace JuegoGato;

public partial class App : Application
{
	public static CatGameViewModel Viewmodel { get; set; } = new();
	public App()
	{
		InitializeComponent();
		Routing.RegisterRoute("//Juego", typeof(GatoView));
        Routing.RegisterRoute("//Score", typeof(ScoreView));
        MainPage = new AppShell();

    }
}
