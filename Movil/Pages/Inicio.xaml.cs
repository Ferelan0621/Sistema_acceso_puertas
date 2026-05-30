using CommunityToolkit.Mvvm.Input;
using Movil.ViewModels;
using Shared.Models;
namespace Movil.Pages;

public partial class Inicio : ContentPage
{
    public Inicio()
    {
        InitializeComponent();
        BindingContext = new InicioViewModel();

    }

}