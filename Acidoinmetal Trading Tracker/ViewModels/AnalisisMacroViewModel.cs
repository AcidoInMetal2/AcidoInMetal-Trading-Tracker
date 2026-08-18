using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Acidoinmetal_Trading_Tracker.ViewModels
{
    public class AnalisisMacroViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly int _sesionId;

        public AnalisisMacroViewModel(DatabaseService databaseService, int sesionId)
        {
            _databaseService = databaseService;
            _sesionId = sesionId;

            // Los campos de EUR y GBP que carguemos de acá en adelante
            // se van a guardar contra este mismo _sesionId, para que
            // queden agrupados con Trader Status bajo la misma Fecha.
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
