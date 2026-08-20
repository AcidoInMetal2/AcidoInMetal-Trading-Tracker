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
        public BloqueAnalisisViewModel EurMicro { get; } = new(OpcionesMarcoMicro);
        public BloqueAnalisisViewModel GbpMicro { get; } = new(OpcionesMarcoMicro);

        public AnalisisMicroViewModel(DatabaseService databaseService, int sesionId)
        {
            _databaseService = databaseService;
            _sesionId = sesionId;

            // TODO: cuando definamos cómo persistir estos datos, guardar
            // EurMicro.Link / GbpMicro.Link contra este mismo _sesionId, para
            // que queden agrupados con el resto bajo la misma Fecha.
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
