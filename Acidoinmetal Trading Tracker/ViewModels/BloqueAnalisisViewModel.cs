using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        private async Task CargarImagenAsync()
        {
            if (string.IsNullOrWhiteSpace(Link))
            {
                ImagenPreview = null;
                Estado = "Pegá un link de TradingView abajo";
                return;
            }

            ImagenPreview = null;
            Estado = "Cargando imagen...";

            string? urlImagen = await _imagenService.ObtenerUrlImagenAsync(Link);

            if (urlImagen == null)
            {
                Estado = "No se pudo cargar la imagen. Revisá el link.";
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
                Estado = string.Empty;
            }
            catch
            {
                Estado = "El link no devolvió una imagen válida.";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
