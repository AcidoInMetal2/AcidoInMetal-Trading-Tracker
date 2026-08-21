namespace Acidoinmetal_Trading_Tracker.Models
{
    /// <summary>
    /// Datos de Análisis Macro o Micro para un Par (EUR/GBP) dentro de una
    /// Sesión (Fecha). La combinación SesionId + Par + Tipo es única: solo
    /// puede haber un registro por Fecha + Par + Tipo.
    /// </summary>
    public class AnalisisPar
    {
        public int Id { get; set; }
        public int SesionId { get; set; }

        // "EUR" o "GBP"
        public string Par { get; set; } = string.Empty;

        // "MACRO" o "MICRO"
        public string Tipo { get; set; } = string.Empty;

        public string Marco { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;

        // "1-2", "2-3", "3-4", o null si no hay ninguno seleccionado
        public string? RangoOperativo { get; set; }

        public string? EstadoRango { get; set; }
        public string Direccion { get; set; } = "SIN DEFINIR";
        public string Comentarios { get; set; } = string.Empty;
    }
}
