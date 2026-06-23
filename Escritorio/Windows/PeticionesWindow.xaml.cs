using Escritorio.Data;
using Shared.Services;
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
    }
}
