namespace JuegoGato.Views;

public partial class ScoreView : ContentPage
{
	public ScoreView()
	{
		InitializeComponent();
		this.BindingContext = App.Viewmodel;
	}
}