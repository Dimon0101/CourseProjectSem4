using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CourseProjectSem4.Base
{
    class PriceConfig : INotifyPropertyChanged
    {
        public static PriceConfig Instance { get; } = new();
        private PriceConfig() { }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        private float _poorRoomPrice = 100f;
        public float PoorRoomPrice
        {
            get => _poorRoomPrice;
            set { if (value > 0) { _poorRoomPrice = value; Notify(); } }
        }

        private float _normalRoomPrice = 250f;
        public float NormalRoomPrice
        {
            get => _normalRoomPrice;
            set { if (value > 0) { _normalRoomPrice = value; Notify(); } }
        }

        private float _deluxeRoomPrice = 500f;
        public float DeluxeRoomPrice
        {
            get => _deluxeRoomPrice;
            set { if (value > 0) { _deluxeRoomPrice = value; Notify(); } }
        }

        private float _wifiPrice = 30f;
        public float WiFiPrice
        {
            get => _wifiPrice;
            set { if (value >= 0) { _wifiPrice = value; Notify(); } }
        }

        private float _allInclusivePrice = 150f;
        public float AllInclusivePrice
        {
            get => _allInclusivePrice;
            set { if (value >= 0) { _allInclusivePrice = value; Notify(); } }
        }
    }
}
