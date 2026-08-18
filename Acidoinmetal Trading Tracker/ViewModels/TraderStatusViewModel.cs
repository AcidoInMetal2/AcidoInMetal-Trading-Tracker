using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Acidoinmetal_Trading_Tracker.Models;
using Acidoinmetal_Trading_Tracker.Services;

namespace Acidoinmetal_Trading_Tracker.ViewModels
{
    public class TraderStatusViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly int _sesionId;

        private int _descanso = 1;
        private int _estadoAnimico = 1;
        private int _nivelStress = 10;
        private int _nivelAnsiedad = 10;
        private int _cabinaEsteril = 1;
        private bool _confirmado = false;

        public ObservableCollection<EstrellaItem> EstrellasDescanso { get; } = CrearEstrellas();
        public ObservableCollection<EstrellaItem> EstrellasEstadoAnimico { get; } = CrearEstrellas();
        public ObservableCollection<EstrellaItem> EstrellasNivelStress { get; } = CrearEstrellas();
        public ObservableCollection<EstrellaItem> EstrellasNivelAnsiedad { get; } = CrearEstrellas();
        public ObservableCollection<EstrellaItem> EstrellasCabinaEsteril { get; } = CrearEstrellas();

        private static ObservableCollection<EstrellaItem> CrearEstrellas()
        {
            var lista = new ObservableCollection<EstrellaItem>();
            for (int i = 1; i <= 10; i++) lista.Add(new EstrellaItem(i));
            return lista;
        }

        public bool Confirmado
        {
            get => _confirmado;
            private set { _confirmado = value; OnPropertyChanged(); OnPropertyChanged(nameof(EtiquetaBoton)); }
        }

        public string EtiquetaBoton => Confirmado ? "Editar" : "Confirmar";

        public int Porcentaje
        {
            get
            {
                int aporteStress = 11 - _nivelStress;
                int aporteAnsiedad = 11 - _nivelAnsiedad;

                int totalAportes = _descanso + _estadoAnimico + aporteStress + aporteAnsiedad + _cabinaEsteril;
                double porcentaje = ((totalAportes - 5) / 45.0) * 100;
                return (int)System.Math.Round(porcentaje);
            }
        }

        public Brush ColorEstadoBrush
        {
            get
            {
                if (Porcentaje < 30) return new SolidColorBrush(Color.FromRgb(0xE2, 0x4B, 0x4A));
                if (Porcentaje < 60) return new SolidColorBrush(Color.FromRgb(0xFA, 0xC7, 0x75));
                return new SolidColorBrush(Color.FromRgb(0x63, 0x99, 0x22));
            }
        }

        public RelayCommand<int> SeleccionarDescansoCommand { get; }
        public RelayCommand<int> SeleccionarEstadoAnimicoCommand { get; }
        public RelayCommand<int> SeleccionarNivelStressCommand { get; }
        public RelayCommand<int> SeleccionarNivelAnsiedadCommand { get; }
        public RelayCommand<int> SeleccionarCabinaEsterilCommand { get; }
        public RelayCommand ConfirmarCommand { get; }

        public TraderStatusViewModel(DatabaseService databaseService, int sesionId)
        {
            _databaseService = databaseService;
            _sesionId = sesionId;

            SeleccionarDescansoCommand = new RelayCommand<int>(n => { _descanso = n; ActualizarEstrellas(EstrellasDescanso, n); OnCambio(); }, _ => !Confirmado);
            SeleccionarEstadoAnimicoCommand = new RelayCommand<int>(n => { _estadoAnimico = n; ActualizarEstrellas(EstrellasEstadoAnimico, n); OnCambio(); }, _ => !Confirmado);
            SeleccionarNivelStressCommand = new RelayCommand<int>(n => { _nivelStress = n; ActualizarEstrellas(EstrellasNivelStress, n); OnCambio(); }, _ => !Confirmado);
            SeleccionarNivelAnsiedadCommand = new RelayCommand<int>(n => { _nivelAnsiedad = n; ActualizarEstrellas(EstrellasNivelAnsiedad, n); OnCambio(); }, _ => !Confirmado);
            SeleccionarCabinaEsterilCommand = new RelayCommand<int>(n => { _cabinaEsteril = n; ActualizarEstrellas(EstrellasCabinaEsteril, n); OnCambio(); }, _ => !Confirmado);

            ConfirmarCommand = new RelayCommand(AlternarConfirmacion);

            InicializarEstrellas(EstrellasDescanso, _descanso);
            InicializarEstrellas(EstrellasEstadoAnimico, _estadoAnimico);
            InicializarEstrellas(EstrellasNivelStress, _nivelStress);
            InicializarEstrellas(EstrellasNivelAnsiedad, _nivelAnsiedad);
            InicializarEstrellas(EstrellasCabinaEsteril, _cabinaEsteril);
        }

        private static void InicializarEstrellas(ObservableCollection<EstrellaItem> estrellas, int valor)
        {
            foreach (var e in estrellas) e.Marcada = e.Numero <= valor;
        }

        private static void ActualizarEstrellas(ObservableCollection<EstrellaItem> estrellas, int valor)
        {
            foreach (var e in estrellas) e.Marcada = e.Numero <= valor;
        }

        private void AlternarConfirmacion()
        {
            if (!Confirmado)
            {
                var datos = new SesionOperativa
                {
                    Descanso = _descanso,
                    EstadoAnimico = _estadoAnimico,
                    NivelStress = _nivelStress,
                    NivelAnsiedad = _nivelAnsiedad,
                    CabinaEsteril = _cabinaEsteril
                };
                _databaseService.GuardarTraderStatus(_sesionId, datos);
                Confirmado = true;
            }
            else
            {
                Confirmado = false;
            }
        }

        private void OnCambio()
        {
            OnPropertyChanged(nameof(Porcentaje));
            OnPropertyChanged(nameof(ColorEstadoBrush));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}