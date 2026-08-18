using System.ComponentModel;

namespace Acidoinmetal_Trading_Tracker.ViewModels
{
    public class EstrellaItem : INotifyPropertyChanged
    {
        public int Numero { get; }

        private bool _marcada;
        public bool Marcada
        {
            get => _marcada;
            set { _marcada = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Marcada))); }
        }

        public EstrellaItem(int numero)
        {
            Numero = numero;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}