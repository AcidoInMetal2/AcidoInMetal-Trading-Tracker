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
            int sesionId = _databaseService.CrearSesion(DateTime.Now);

            VistaTraderStatus.DataContext = new TraderStatusViewModel(_databaseService, sesionId);
            VistaAnalisisMacro.DataContext = new AnalisisMacroViewModel(_databaseService, sesionId);
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

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            PanelTraderStatus.Visibility = Visibility.Collapsed;
            PanelAnalisisMacro.Visibility = Visibility.Collapsed;
            PanelDashboard.Visibility = Visibility.Visible;
            TxtTituloPantalla.Text = "Dashboard";
        }
    }
}
