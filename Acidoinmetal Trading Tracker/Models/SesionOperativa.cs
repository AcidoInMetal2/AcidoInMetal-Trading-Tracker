using System;

namespace Acidoinmetal_Trading_Tracker.Models
{
    public class SesionOperativa
    {
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }

        // Trader Status
        public int Descanso { get; set; } = 1;
        public int EstadoAnimico { get; set; } = 1;
        public int NivelStress { get; set; } = 1;
        public int NivelAnsiedad { get; set; } = 1;
        public int CabinaEsteril { get; set; } = 1;

        public bool TraderStatusConfirmado { get; set; } = false;

        public int PorcentajeTraderStatus()
        {
            int total = Descanso + EstadoAnimico + NivelStress + NivelAnsiedad + CabinaEsteril;
            return (int)Math.Round((total / 50.0) * 100);
        }
    }
}