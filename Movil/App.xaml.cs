using Microsoft.Extensions.DependencyInjection;
using Movil.Pages;
using Movil.Data;

namespace Movil
{
    public partial class App : Application
    {

        ConexionMqtt conexion = new();
        public App()
        {
            InitializeComponent();
            //MainPage = new NavigationPage(new IniciarSesion());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {

            return new Window(new MyShell());
        }
    }
}