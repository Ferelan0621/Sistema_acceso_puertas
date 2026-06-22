using Escritorio.Data;
using Escritorio.Mvvm;
using Shared.Models;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Escritorio.Windows
{
    public partial class LaboratorioWindow : Window
    {
        public LaboratorioWindow()
        {
            InitializeComponent();
        }

        private void btnImagenregresar_Click(object sender, RoutedEventArgs e)
        {
            // Obtienes el ViewModel actual y apagas el SSE
            if (this.DataContext is LaboratorioViewModel vm)
            {
                vm.DesconectarSSE();
            }

            try
            {
                InicioWindow ventanaInicio = new InicioWindow();
                ventanaInicio.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la ventana: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}