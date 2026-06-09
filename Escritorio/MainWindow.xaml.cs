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
using Shared.Services;
using Shared.Models;
using System.Text.Json;
using Escritorio.Data;

namespace Escritorio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly ApiService _apiService;
        public MainWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            /* Ocultar etiquetas de error al inicio por si acaso
            lblFaltausuario.Visibility = Visibility.Hidden;
            lblFaltacontrasenia.Visibility = Visibility.Hidden;

            string usuarioCampo = txtUsuario.Text.Trim();
            string contraseniaCampo = txtContrasenia.Password.Trim();
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
                try
                {
                    // 1. Instanciamos el servicio que te dije que crearas antes
                    EncargadosService servicio = new EncargadosService();

                    // 2. Llamamos a la API y esperamos la respuesta
                    Encargados usuarioValido = await servicio.Login(usuarioCampo, contraseniaCampo);

                    // 3. Verificamos si la API nos devolvió datos (credenciales correctas)
                    if (usuarioValido != null)
                    {
                        // ¡Éxito! Ahora sí lo dejamos pasar
                        // Pásale el objeto usuarioValido si InicioWindow lo necesita
                        InicioWindow ventaInicio = new InicioWindow();
                        ventaInicio.Show();
                        this.Hide();
                    }
                    else
                    {
                        // ¡Rechazado! 
                        MessageBox.Show("¡Usuario o contraseña incorrectos! Revisa bien tus datos.", "Error de Autenticación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    // Por si tu API está apagada o explota algo
                    MessageBox.Show($"¡El servidor no responde! ¿Encendiste la API? Error: {ex.Message}", "Fallo de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }*/
            string usuario = txtUsuario.Text.Trim();
            string password = txtContrasenia.Password.Trim();

            // Validaciones básicas antes de gastar datos/red
            /*if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                lblFaltausuario.Visibility = Visibility.Visible;
                lblFaltacontrasenia.Visibility = Visibility.Visible;
            }

            try
            {
                // Llamamos a nuestro servicio que hace el POST al Dev Tunnel
                bool loginExitoso = await _apiService.IniciarSesionAsync(usuario, password);

                if (loginExitoso)
                {
                    InicioWindow ventaInicio = new InicioWindow();
                    ventaInicio.Show();
                    this.Hide();
                }
                else
                {
                    // El servidor regresó 401 Unauthorized o falló la validación
                    MessageBox.Show("¡Usuario o contraseña incorrectos! Revisa bien tus datos.", "Error de Autenticación", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                // Por si el servidor está apagado o el Dev Tunnel expiró
                MessageBox.Show($"¡El servidor no responde! ¿Encendiste la API? Error: {ex.Message}", "Fallo de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/
            InicioWindow ventaInicio = new InicioWindow();
            ventaInicio.Show();
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