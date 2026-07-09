using System.Windows;

namespace Escritorio.Windows
{
	/// <summary>
	/// Lógica de interacción para PeticionDialogoWindow.xaml
	/// </summary>
	public partial class PeticionDialogoWindow : Window
	{
		public PeticionDialogoWindow()
		{
			InitializeComponent();
		}

		private void CerrarVentana_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}