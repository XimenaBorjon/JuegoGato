using JuegoGato.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;

namespace JuegoGato.Views;

public partial class GatoView : ContentPage
{
	public GatoView()
	{
		InitializeComponent();
		this.BindingContext = App.Viewmodel;
	}



}