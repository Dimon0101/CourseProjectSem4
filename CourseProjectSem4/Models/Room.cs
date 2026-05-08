using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CourseProjectSem4.Models
{
    abstract class Room : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Notify([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        static int roomNumber = 0;

        public int Id { get; init; }

        private string _owner = "";
        public string Owner
        {
            get => _owner;
            set { _owner = value ?? ""; Notify(); }
        }

        private bool _isFree = true;
        public bool IsFree
        {
            get => _isFree;
            set { _isFree = value; Notify(); }
        }

        private float _price;
        public float Price
        {
            get => _price;
            set { if (value > 0) { _price = value; Notify(); } }
        }

        public bool HasWiFi      { get; set; }
        public bool AllInclusive { get; set; }

        protected Room(int id, string owner, float price)
        {
            Id    = roomNumber++;
            Owner = owner;
            Price = price;
        }
    }

    class PoorRoom : Room
    {
        public PoorRoom(int id, string owner, float price) : base(id, owner, price) { }
    }

    class NormalRoom : Room
    {
        public NormalRoom(int id, string owner, float price) : base(id, owner, price) { }
    }

    class DeluxeRoom : Room
    {
        public DeluxeRoom(int id, string owner, float price) : base(id, owner, price) { }
    }
}
