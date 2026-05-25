using Escritorio.Windows;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Escritorio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.ToString();

            string usuarioCampo = txtUsuario.Text.Trim();
            string contraseniaCampo = txtContrasenia.Text.Trim();
            bool datosCompletos = true;

            if (string.IsNullOrEmpty(usuarioCampo))
            {
                lblFaltausuario.Visibility = Visibility.Visible;
                datosCompletos = false;
            }
            if (string.IsNullOrEmpty(contraseniaCampo))
            {
                lblFaltacontrasenia.Visibility = Visibility.Visible;
                datosCompletos = false;
            }
            if (datosCompletos)
            {
                InicioWindow ventaInicio = new InicioWindow(usuario);
                ventaInicio.Show();
                this.Hide();
            }
            
        }

        private void btnRegistrarse_Click(object sender, RoutedEventArgs e)
        {
            RegistrarseWindow ventanaRegistrarse = new RegistrarseWindow();
            ventanaRegistrarse.Show();
            this.Hide();
        }
        private void lblContrasenia(object sender, RoutedEventArgs e)
        {
            RecuperarWindow ventanaRecuperar = new RecuperarWindow();
            ventanaRecuperar.Show();
            this.Hide();
        }
    }
}