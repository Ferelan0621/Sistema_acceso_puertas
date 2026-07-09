using System.Configuration;
using System.Data;
using System.Windows;
using Escritorio.Data;

namespace Escritorio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Esto arranca el listener de MQTT en segundo plano al abrir el programa
			_ = SharedData.Instance.PeticionesVM.IniciarTodo();
		}
	}



}
