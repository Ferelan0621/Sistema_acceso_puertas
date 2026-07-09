using System; // Necesario para Lazy<T>
using System.Collections.ObjectModel;
using Shared.Models;

namespace Escritorio.Data
{
	public class SharedData
	{
		private static SharedData _instance;
		public static SharedData Instance => _instance ??= new SharedData();

		public EscritorioMQTT Broker { get; } = new EscritorioMQTT();
		public ObservableCollection<Laboratorios> ListaLaboratorios { get; } = new ObservableCollection<Laboratorios>();

		// Usamos Lazy para evitar la referencia circular en el constructor
		private readonly Lazy<ViewModel.PeticionesViewModel> _peticionesVM =
			new Lazy<ViewModel.PeticionesViewModel>(() => new ViewModel.PeticionesViewModel());

		public ViewModel.PeticionesViewModel PeticionesVM => _peticionesVM.Value;

		private SharedData() { }
	}
}