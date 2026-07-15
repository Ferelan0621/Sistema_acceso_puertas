
using CommunityToolkit.Mvvm.ComponentModel;

namespace Shared.Models
{
    public  partial class Prestamos : ObservableObject
    {
        public int ID { get; set; }

        public int UsuarioID { get; set; }

        [ObservableProperty]
        private Usuarios? usuario;

        public int LaboratorioID { get; set; }
        [ObservableProperty]
        private Laboratorios? laboratorio;

        [ObservableProperty]
        private DateTime fechaSolicitud;
        [ObservableProperty]
        private DateTime fechaApertura;
        [ObservableProperty]
        private DateTime fechaCierre;
        [ObservableProperty]
        private DateTime fechaCierreRemoto;
        public int EncargadoID { get; set; }
        [ObservableProperty]
        private Encargados? encargado;

    }
}
