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
    /// Lógica de interacción para InicioWindow.xaml
    /// </summary>
    public partial class InicioWindow : Window
    {
        public InicioWindow(string usuario)
        {
            InitializeComponent();
            lblUsuario.Content = "Bienvenido " + usuario;
        }

        private void btnLaboratorio_Click(object sender, RoutedEventArgs e)
        {
            LaboratorioWindow ventanaLaboratorio = new LaboratorioWindow();
            ventanaLaboratorio.Show();
            this.Hide();
        }

        private void btnPeticiones_Click(object sender, RoutedEventArgs e)
        {
            PeticionesWindow ventanaPeticiones = new PeticionesWindow();
            ventanaPeticiones.Show();
            this.Hide();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            UsuariosWindow ventanaUsuarios = new UsuariosWindow();
            ventanaUsuarios.Show();
            this.Hide();
        }

        private void btnCerrarsesion_Click(object sender, RoutedEventArgs e)
        {
            MainWindow ventanaSesion = new MainWindow();
            ventanaSesion.Show();
            this.Close();
        }
    }
}
