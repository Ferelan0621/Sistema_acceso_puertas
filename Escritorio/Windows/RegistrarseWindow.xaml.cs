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
    /// Lógica de interacción para RegistrarseWindow.xaml
    /// </summary>
    public partial class RegistrarseWindow : Window
    {
        public RegistrarseWindow()
        {
            InitializeComponent();
        }

        private void btnCrearnuevo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow inicioSesion = new MainWindow();
            inicioSesion.Show();
            this.Close();
        }
    }
}
