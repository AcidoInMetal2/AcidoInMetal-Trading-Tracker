using System;
using System.Windows;
using Acidoinmetal_Trading_Tracker.Services;
using Acidoinmetal_Trading_Tracker.ViewModels;

namespace AcidoInMetalTradingTracker
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _databaseService;

        public MainWindow()
        {
            InitializeComponent();

            _databaseService = new DatabaseService();
            int sesionId = _databaseService.ObtenerOCrearSesionPorFecha(DateTime.Now);

            // Precalienta la conexión HTTPS para que la primera carga de imagen
            // de TradingView no pague el costo de la conexión "en frío".
            _ = TradingViewImageService.PrecalentarAsync();

            VistaTraderStatus.DataContext = new TraderStatusViewModel(_databaseService, sesionId);
            VistaAnalisisMacro.DataContext = new AnalisisMacroViewModel(_databaseService, sesionId);
            VistaAnalisisMicro.DataContext = new AnalisisMicroViewModel(_databaseService, sesionId);
        }

        private void BtnIrATraderStatus_Click(object sender, RoutedEventArgs e)
        {
            PanelDashboard.Visibility = Visibility.Collapsed;
            PanelTraderStatus.Visibility = Visibility.Visible;
            TxtTituloPantalla.Text = "Registro Operativo";
        }

        private void BtnIrAAnalisisMacro_Click(object sender, RoutedEventArgs e)
        {
            PanelDashboard.Visibility = Visibility.Collapsed;
            PanelAnalisisMacro.Visibility = Visibility.Visible;
            TxtTituloPantalla.Text = "Análisis Macro";
        }

        private void BtnIrAAnalisisMicro_Click(object sender, RoutedEventArgs e)
        {
            PanelDashboard.Visibility = Visibility.Collapsed;
            PanelAnalisisMicro.Visibility = Visibility.Visible;
            TxtTituloPantalla.Text = "Análisis Micro";
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            PanelTraderStatus.Visibility = Visibility.Collapsed;
            PanelAnalisisMacro.Visibility = Visibility.Collapsed;
            PanelAnalisisMicro.Visibility = Visibility.Collapsed;
            PanelDashboard.Visibility = Visibility.Visible;
            TxtTituloPantalla.Text = "Dashboard";
        }
    }
}
