using System;
using System.ComponentModel;
using System.Diagnostics;
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

        // Por ahora queda fijo en "H1". El nombre se mantiene "Macroview1" porque
        // a futuro esto va a pasar a ser un ComboBox editable con varias opciones
        // (H1, H4, D1, etc.) sin tener que renombrar la propiedad.
        private string _macroview1 = "H1";
        public string Macroview1
        {
            get => _macroview1;
            set { _macroview1 = value; OnPropertyChanged(); }
        }

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
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(urlImagen, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
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
