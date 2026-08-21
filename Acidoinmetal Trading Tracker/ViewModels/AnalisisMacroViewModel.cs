using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Acidoinmetal_Trading_Tracker.Services;

namespace Acidoinmetal_Trading_Tracker.ViewModels
{
    public class AnalisisMacroViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly int _sesionId;

        private static readonly string[] OpcionesMarcoMacro = { "H1", "H4", "Diario", "Mensual" };

        // Bloque de cada divisa, con las opciones de marco de Análisis Macro.
        public BloqueAnalisisViewModel EurH1 { get; }
        public BloqueAnalisisViewModel GbpH1 { get; }

        public AnalisisMacroViewModel(DatabaseService databaseService, int sesionId)
        {
            _databaseService = databaseService;
            _sesionId = sesionId;

            EurH1 = new BloqueAnalisisViewModel(OpcionesMarcoMacro, databaseService, sesionId, "EUR", "MACRO");
            GbpH1 = new BloqueAnalisisViewModel(OpcionesMarcoMacro, databaseService, sesionId, "GBP", "MACRO");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
