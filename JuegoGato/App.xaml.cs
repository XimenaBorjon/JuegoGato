using JuegoGato.Views;

namespace JuegoGato;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		App.Current.MainPage = new RegistroJugador();


    }
}
