using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Acidoinmetal_Trading_Tracker.Services;

namespace Acidoinmetal_Trading_Tracker.ViewModels
{
    /// <summary>
    /// Representa un bloque de carga dentro de Análisis Macro: un marco temporal
    /// (hoy fijo en "H1", a futuro un desplegable), el link de TradingView, y la
    /// imagen de preview obtenida a partir de ese link.
    /// </summary>
    public class BloqueAnalisisViewModel : INotifyPropertyChanged
    {
        private readonly TradingViewImageService _imagenService = new();

        // Por ahora queda fijo en "H1" al arrancar. El nombre se mantiene
        // "Macroview1" por pedido explícito, pensando en la evolución futura
        // del concepto (Macroview2, etc. si hiciera falta más adelante).
        private string _macroview1 = "H1";
        public string Macroview1
        {
            get => _macroview1;
            set { _macroview1 = value; OnPropertyChanged(); }
        }

        // Opciones fijas del desplegable de Marco temporal.
        public string[] OpcionesMarco { get; } = { "H1", "H4", "Diario", "Mensual" };

        private string _link = string.Empty;
        public string Link
        {
            get => _link;
            set
            {
                if (_link == value) return;
                _link = value;
                OnPropertyChanged();
                _ = CargarImagenAsync();
            }
        }

        private BitmapImage? _imagenPreview;
        public BitmapImage? ImagenPreview
        {
            get => _imagenPreview;
            set { _imagenPreview = value; OnPropertyChanged(); }
        }

        // Mensaje que se muestra en la caja de preview mientras no hay imagen
        // (vacío, cargando, o error). Cuando hay imagen cargada, queda en blanco.
        private string _estado = "Pegá un link de TradingView abajo";
        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        // Controla si se muestra el botón "Reintentar" (solo cuando hubo un error real).
        private bool _mostrarReintentar;
        public bool MostrarReintentar
        {
            get => _mostrarReintentar;
            set { _mostrarReintentar = value; OnPropertyChanged(); }
        }

        // Permite reintentar aunque el link no haya cambiado (por ejemplo,
        // si falló por timeout la primera vez).
        public ICommand ReintentarCommand { get; }

        // URL directa del .png (la que sacamos del meta-tag og:image). Se usa
        // para poder abrirla en el navegador con un click sobre la imagen.
        private string? _urlImagenDirecta;
        public string? UrlImagenDirecta
        {
            get => _urlImagenDirecta;
            private set { _urlImagenDirecta = value; OnPropertyChanged(); }
        }

        public ICommand AbrirImagenCommand { get; }

        // ===================== Rango Operativo =====================
        // Estructura del gráfico: 1-2 = impulso inicial, 2-3 = retroceso,
        // 3-4 = nuevo impulso. Selección excluyente entre las tres.
        private string? _rangoOperativo;
        public string? RangoOperativo
        {
            get => _rangoOperativo;
            set
            {
                _rangoOperativo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Rango12));
                OnPropertyChanged(nameof(Rango23));
                OnPropertyChanged(nameof(Rango34));
            }
        }

        public bool Rango12
        {
            get => RangoOperativo == "1-2";
            set => RangoOperativo = value ? "1-2" : (RangoOperativo == "1-2" ? null : RangoOperativo);
        }

        public bool Rango23
        {
            get => RangoOperativo == "2-3";
            set => RangoOperativo = value ? "2-3" : (RangoOperativo == "2-3" ? null : RangoOperativo);
        }

        public bool Rango34
        {
            get => RangoOperativo == "3-4";
            set => RangoOperativo = value ? "3-4" : (RangoOperativo == "3-4" ? null : RangoOperativo);
        }

        // ===================== Estado del Rango =====================
        public string[] OpcionesEstadoRango { get; } = { "Iniciado", "En Progreso", "Finalizando", "A Confirmar" };

        private string? _estadoRango;
        public string? EstadoRango
        {
            get => _estadoRango;
            set { _estadoRango = value; OnPropertyChanged(); }
        }

        // ===================== Dirección =====================
        public string[] OpcionesDireccion { get; } = { "COMPRAS", "VENTAS", "SIN DEFINIR" };

        private string _direccion = "SIN DEFINIR";
        public string Direccion
        {
            get => _direccion;
            set { _direccion = value; OnPropertyChanged(); }
        }

        // ===================== Comentarios =====================
        private string _comentarios = string.Empty;
        public string Comentarios
        {
            get => _comentarios;
            set { _comentarios = value; OnPropertyChanged(); }
        }

        public BloqueAnalisisViewModel()
        {
            ReintentarCommand = new RelayCommand(() => _ = CargarImagenAsync());
            AbrirImagenCommand = new RelayCommand(AbrirImagenEnNavegador, () => !string.IsNullOrEmpty(UrlImagenDirecta));
        }

        private void AbrirImagenEnNavegador()
        {
            if (string.IsNullOrEmpty(UrlImagenDirecta)) return;
            try
            {
                Process.Start(new ProcessStartInfo(UrlImagenDirecta) { UseShellExecute = true });
            }
            catch
            {
                // Si no se puede abrir el navegador por algún motivo del sistema,
                // no queremos romper la app por eso.
            }
        }

        private async Task CargarImagenAsync()
        {
            if (string.IsNullOrWhiteSpace(Link))
            {
                ImagenPreview = null;
                UrlImagenDirecta = null;
                Estado = "Pegá un link de TradingView abajo";
                MostrarReintentar = false;
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            ImagenPreview = null;
            UrlImagenDirecta = null;
            Estado = "Cargando imagen...";
            MostrarReintentar = false;
            CommandManager.InvalidateRequerySuggested();

            string? urlImagen = await _imagenService.ObtenerUrlImagenAsync(Link);

            if (urlImagen == null)
            {
                Estado = "No se pudo cargar la imagen. Revisá el link.";
                MostrarReintentar = true;
                return;
            }

            try
            {
                byte[]? bytesImagen = await _imagenService.DescargarBytesAsync(urlImagen);

                if (bytesImagen == null)
                {
                    Estado = "Se encontró la imagen pero no se pudo descargar. Probá reintentar.";
                    MostrarReintentar = true;
                    return;
                }

                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(bytesImagen))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                bitmap.Freeze();

                ImagenPreview = bitmap;
                UrlImagenDirecta = urlImagen;
                Estado = string.Empty;
                MostrarReintentar = false;
            }
            catch
            {
                Estado = "El link no devolvió una imagen válida.";
                MostrarReintentar = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
