using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Acidoinmetal_Trading_Tracker.Services;

namespace Acidoinmetal_Trading_Tracker.ViewModels
{
    public class AnalisisMicroViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly int _sesionId;

        private static readonly string[] OpcionesMarcoMicro = { "M15", "M5", "M3", "M2" };

        // Bloque de cada divisa, con las opciones de marco de Análisis Micro.
        public BloqueAnalisisViewModel EurMicro { get; }
        public BloqueAnalisisViewModel GbpMicro { get; }

        public AnalisisMicroViewModel(DatabaseService databaseService, int sesionId)
        {
            _databaseService = databaseService;
            _sesionId = sesionId;

            EurMicro = new BloqueAnalisisViewModel(OpcionesMarcoMicro, databaseService, sesionId, "EUR", "MICRO");
            GbpMicro = new BloqueAnalisisViewModel(OpcionesMarcoMicro, databaseService, sesionId, "GBP", "MICRO");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
