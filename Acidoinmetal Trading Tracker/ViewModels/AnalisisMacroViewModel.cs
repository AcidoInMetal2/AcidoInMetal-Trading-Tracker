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

        // Bloque H1 de cada divisa. A futuro se van a sumar más marcos
        // temporales (H4, D1, etc.) siguiendo el mismo patrón.
        public BloqueAnalisisViewModel EurH1 { get; } = new();
        public BloqueAnalisisViewModel GbpH1 { get; } = new();

        public AnalisisMacroViewModel(DatabaseService databaseService, int sesionId)
        {
            _databaseService = databaseService;
            _sesionId = sesionId;

            // TODO: cuando definamos cómo persistir estos datos, guardar
            // EurH1.Link / GbpH1.Link contra este mismo _sesionId, para que
            // queden agrupados con Trader Status bajo la misma Fecha.
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
