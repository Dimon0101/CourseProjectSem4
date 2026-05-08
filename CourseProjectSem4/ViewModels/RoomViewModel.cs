using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CourseProjectSem4.Base;
using CourseProjectSem4.Models;

namespace CourseProjectSem4.ViewModels
{
    enum RoomStatus { Free, Reserved, Occupied }

    class RoomViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void Notify([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        public Room        Room    { get; }
        public RoomQuality Quality { get; }
        public int         DbId    { get; }

        public string DisplayNumber   => $"№ {Room.Id + 1}";
        public float  BasePrice       => Room.Price;
        public bool   HasWiFi         => Room.HasWiFi;
        public bool   HasAllInclusive => Room.AllInclusive;

        public string TypeName       { get; }
        public Brush  TypeBrush      { get; }
        public Brush  TypeLightBrush { get; }

        private RoomStatus _status = RoomStatus.Free;
        public RoomStatus Status
        {
            get => _status;
            set
            {
                _status     = value;
                Room.IsFree = value == RoomStatus.Free;
                Notify();
                Notify(nameof(IsOccupied));
                Notify(nameof(CanBook));
                Notify(nameof(CanDelete));
                Notify(nameof(StatusText));
                Notify(nameof(StatusBrush));
                Notify(nameof(StatusTextBrush));
            }
        }

        // Зворотня сумісність
        public bool IsOccupied
        {
            get => _status != RoomStatus.Free;
            set => Status = value ? RoomStatus.Occupied : RoomStatus.Free;
        }

        public bool CanBook   => _status == RoomStatus.Free;
        public bool CanDelete => _status == RoomStatus.Free;

        public string StatusText => _status switch
        {
            RoomStatus.Reserved => "Зарезервована",
            RoomStatus.Occupied => "Зайнята",
            _                   => "Вільна"
        };

        public Brush StatusBrush => _status switch
        {
            RoomStatus.Reserved => new SolidColorBrush(Color.FromRgb(255, 248, 225)),
            RoomStatus.Occupied => new SolidColorBrush(Color.FromRgb(253, 237, 237)),
            _                   => new SolidColorBrush(Color.FromRgb(232, 245, 233))
        };

        public Brush StatusTextBrush => _status switch
        {
            RoomStatus.Reserved => new SolidColorBrush(Color.FromRgb(180, 120, 0)),
            RoomStatus.Occupied => new SolidColorBrush(Color.FromRgb(198, 40, 40)),
            _                   => new SolidColorBrush(Color.FromRgb(46, 125, 50))
        };

        public RoomViewModel(Room room, RoomQuality quality, int dbId = 0)
        {
            Room    = room;
            Quality = quality;
            DbId    = dbId;

            (TypeName, TypeBrush, TypeLightBrush) = quality switch
            {
                RoomQuality.Poor => (
                    "Бюджетний",
                    (Brush)new SolidColorBrush(Color.FromRgb(96, 125, 139)),
                    new SolidColorBrush(Color.FromRgb(236, 239, 241))
                ),
                RoomQuality.Normal => (
                    "Стандарт",
                    new SolidColorBrush(Color.FromRgb(66, 165, 245)),
                    new SolidColorBrush(Color.FromRgb(227, 242, 253))
                ),
                _ => (
                    "Делюкс",
                    new SolidColorBrush(Color.FromRgb(255, 179, 0)),
                    new SolidColorBrush(Color.FromRgb(255, 248, 225))
                )
            };
        }

        public override string ToString()
            => $"{DisplayNumber} — {TypeName}  ({BasePrice} грн/ніч)";
    }
}
