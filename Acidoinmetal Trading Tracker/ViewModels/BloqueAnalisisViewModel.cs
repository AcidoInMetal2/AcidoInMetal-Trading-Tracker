using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Acidoinmetal_Trading_Tracker.Models;
using Acidoinmetal_Trading_Tracker.Services;

namespace Acidoinmetal_Trading_Tracker.ViewModels
{
    /// <summary>
    /// Representa un bloque de carga dentro de Análisis Macro/Micro: marco
    /// temporal, link de TradingView + preview, Rango Operativo, Estado del
    /// Rango, Dirección y Comentarios. Se guarda solo (upsert) contra
    /// AnalisisPar cada vez que cambia algo, agrupado por Sesión (Fecha) + Par + Tipo.
    /// </summary>
    public class BloqueAnalisisViewModel : INotifyPropertyChanged
    {
        private readonly TradingViewImageService _imagenService = new();
        private readonly DatabaseService _databaseService;
        private readonly int _sesionId;
        private readonly string _par;
        private readonly string _tipo;

        // Se pone en true mientras se restauran los datos guardados al
        // arrancar, para no disparar un guardado innecesario por cada
        // propiedad que se va asignando durante esa carga inicial.
        private bool _cargandoDatos = true;

        // Por ahora arranca en el primer valor de OpcionesMarco. El nombre se
        // mantiene "Macroview1" por pedido explícito, pensando en la evolución
        // futura del concepto (Macroview2, etc. si hiciera falta más adelante).
        private string _macroview1;
        public string Macroview1
        {
            get => _macroview1;
            set { _macroview1 = value; OnPropertyChanged(); OnCambio(); }
        }

        // Opciones del desplegable de Marco temporal. Se reciben por constructor
        // porque este mismo bloque se reusa en Análisis Macro (H1/H4/Diario/Mensual)
        // y en Análisis Micro (M15/M5/M3/M2).
        public string[] OpcionesMarco { get; }

        private string _link = string.Empty;
        public string Link
        {
            get => _link;
            set
            {
                if (_link == value) return;
                _link = value;
                OnPropertyChanged();
                OnCambio();
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
                OnCambio();
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
        public string[] OpcionesEstadoRango { get; } = { "Iniciando", "En Progreso", "Finalizando", "A Confirmar" };

        private string? _estadoRango;
        public string? EstadoRango
        {
            get => _estadoRango;
            set { _estadoRango = value; OnPropertyChanged(); OnCambio(); }
        }

        // ===================== Dirección =====================
        public string[] OpcionesDireccion { get; } = { "COMPRAS", "VENTAS", "SIN DEFINIR" };

        private string _direccion = "SIN DEFINIR";
        public string Direccion
        {
            get => _direccion;
            set { _direccion = value; OnPropertyChanged(); OnCambio(); }
        }

        // ===================== Comentarios =====================
        private string _comentarios = string.Empty;
        public string Comentarios
        {
            get => _comentarios;
            set { _comentarios = value; OnPropertyChanged(); OnCambio(); }
        }

        public BloqueAnalisisViewModel(string[] opcionesMarco, DatabaseService databaseService, int sesionId, string par, string tipo)
        {
            OpcionesMarco = opcionesMarco;
            _macroview1 = opcionesMarco.Length > 0 ? opcionesMarco[0] : string.Empty;
            _databaseService = databaseService;
            _sesionId = sesionId;
            _par = par;
            _tipo = tipo;

            ReintentarCommand = new RelayCommand(() => _ = CargarImagenAsync());
            AbrirImagenCommand = new RelayCommand(AbrirImagenEnNavegador, () => !string.IsNullOrEmpty(UrlImagenDirecta));

            CargarDesdeBaseDeDatos();
        }

        private void CargarDesdeBaseDeDatos()
        {
            var datos = _databaseService.ObtenerAnalisisPar(_sesionId, _par, _tipo);
            if (datos != null)
            {
                if (!string.IsNullOrEmpty(datos.Marco))
                    Macroview1 = datos.Marco;
                RangoOperativo = datos.RangoOperativo;
                EstadoRango = datos.EstadoRango;
                Direccion = string.IsNullOrEmpty(datos.Direccion) ? "SIN DEFINIR" : datos.Direccion;
                Comentarios = datos.Comentarios ?? string.Empty;
                // Al final: si había un link guardado, esto también dispara
                // la recarga de la imagen de preview automáticamente.
                Link = datos.Link ?? string.Empty;
            }

            // Recién ahora se habilita el guardado automático ante cambios.
            _cargandoDatos = false;
        }

        private void OnCambio()
        {
            if (_cargandoDatos) return;

            _databaseService.GuardarAnalisisPar(_sesionId, _par, _tipo, new AnalisisPar
            {
                SesionId = _sesionId,
                Par = _par,
                Tipo = _tipo,
                Marco = Macroview1,
                Link = Link,
                RangoOperativo = RangoOperativo,
                EstadoRango = EstadoRango,
                Direccion = Direccion,
                Comentarios = Comentarios
            });
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
