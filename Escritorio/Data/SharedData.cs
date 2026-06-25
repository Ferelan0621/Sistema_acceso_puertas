using System.Collections.ObjectModel;
using Shared.Models;
using Escritorio.Data;

namespace Escritorio.Data
{
	public class SharedData
	{
		// Instancia única global (Singleton)
		public static SharedData Instance { get; } = new SharedData();

		// Propiedades globales para el MQTT y la lista de laboratorios
		public EscritorioMQTT Broker { get; }
		public ObservableCollection<Laboratorios> ListaLaboratorios { get; }

		private SharedData()
		{
			// Al estar en la misma carpeta Data, encuentra EscritorioMQTT automáticamente
			Broker = new EscritorioMQTT();
			ListaLaboratorios = new ObservableCollection<Laboratorios>();
		}
	}
}