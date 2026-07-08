using Escritorio.ViewModel;
using Shared.Models;
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
    /// Lógica de interacción para PeticionDialogoWindow.xaml
    /// </summary>
    public partial class PeticionDialogoWindow : Window
    {
        public PeticionMovil Peticion { get; set; }
        public PeticionesViewModel ViewModel { get; set; }
        public string NombreUsuario { get; set; }
        public string RolUsuario { get; set; }
        public string NombreLaboratorio { get; set; }

        public PeticionDialogoWindow(PeticionMovil peticion, PeticionesViewModel viewModel, string nombreUser, string rolUser, string nombreLab)
        {
            InitializeComponent();

            Peticion = peticion;
            ViewModel = viewModel;
            NombreUsuario = nombreUser;
            RolUsuario = rolUser;
            NombreLaboratorio = nombreLab;

            this.DataContext = this;
        }
        private void CerrarVentana_Click(object sender, RoutedEventArgs e)
        {
            //Cierra la ventana despues
            this.Close();
        }
    }
}
