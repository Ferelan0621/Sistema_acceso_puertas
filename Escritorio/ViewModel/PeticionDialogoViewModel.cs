using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Models;
using System.Linq;

namespace Escritorio.ViewModel
{
	public partial class PeticionDialogoViewModel : ObservableObject
	{
		public PeticionMovil Peticion { get; }
		public PeticionesViewModel ParentViewModel { get; }

		[ObservableProperty]
		private string nombreUsuario = "Cargando...";

		[ObservableProperty]
		private string rolUsuario = "";

		[ObservableProperty]
		private string nombreLaboratorio;

		public PeticionDialogoViewModel(PeticionMovil peticion, PeticionesViewModel parent)
		{
			Peticion = peticion;
			ParentViewModel = parent;

			if (parent.ListaLaboratorios != null)
			{
				var lab = parent.ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);
				nombreLaboratorio = lab?.NombreLaboratorio ?? $"Laboratorio {peticion.LaboratorioID}";
			}
			else
			{
				nombreLaboratorio = $"Laboratorio {peticion.LaboratorioID}";
			}
		}

		[RelayCommand]
		private void Aceptar()
		{
			// Ejecuta el comando en el ViewModel padre usando la petición actual
			ParentViewModel.AceptarPeticionCommand.Execute(Peticion);
		}

		[RelayCommand]
		private void Denegar()
		{
			// Ejecuta el comando en el ViewModel padre usando la petición actual
			ParentViewModel.DenegarPeticionCommand.Execute(Peticion);
		}
	}
}