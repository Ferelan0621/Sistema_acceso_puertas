using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Escritorio.Data;
// ¡CRÍTICO! Agrega el using de tus ViewModels
using Escritorio.ViewModel;
using Shared.Models;
using Shared.Services;

namespace Escritorio.Windows
{
	/// <summary>
	/// Lógica de interacción para PeticionesWindow.xaml
	/// </summary>
	public partial class PeticionesWindow : Window
	{
		public PeticionesWindow()
		{
			InitializeComponent();

			// En lugar de: this.DataContext = new PeticionesViewModel();
			// Usamos la instancia global que ya está escuchando al MQTT
			this.DataContext = SharedData.Instance.PeticionesVM;
		}

		private void CerrarVentana_Click(object sender, RoutedEventArgs e)
		{

			this.Close();
		}

		private void btnHistorialpeticiones_Click(object sender, RoutedEventArgs e)
		{
			HistorialpeticionesWindow ventanaHistorial = new HistorialpeticionesWindow();
			ventanaHistorial.Show();
			this.Hide();
		}

		private void btnImagenregresar_Click(object sender, RoutedEventArgs e)
		{
			InicioWindow ventanaInicio = new InicioWindow();
			ventanaInicio.Show();
			this.Close();
		}
		public partial class PeticionDialogoWindow : Window
		{
			public PeticionMovil Peticion { get; set; }
			public PeticionesViewModel ViewModel { get; set; }
			public string NombreUsuario { get; set; }
			public string RolUsuario { get; set; }
			public string NombreLaboratorio { get; set; }

			public PeticionDialogoWindow(PeticionMovil peticion, PeticionesViewModel viewModel, string nombreUser, string rolUser, string nombreLab)
			{
				

				Peticion = peticion;
				ViewModel = viewModel;
				NombreUsuario = nombreUser;
				RolUsuario = rolUser;
				NombreLaboratorio = nombreLab;

				this.DataContext = this;
			}
		
	}
}
	}