using CommunityToolkit.Mvvm.ComponentModel;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Movil.ViewModels
{
    // ViewModels/PrestamosViewModel.cs
    [QueryProperty(nameof(LaboratorioActual), "LabSeleccionado")]
    public partial class PrestamosViewModel : ObservableObject
    {
        [ObservableProperty]
        private Laboratorios laboratorioActual;

        // Aquí ya puedes usar LaboratorioActual.Nombre, etc., para armar tu vista de agendar hora
    }
}
