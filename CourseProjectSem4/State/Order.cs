using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CourseProjectSem4.Models;
using CourseProjectSem4.Observers;

namespace CourseProjectSem4.States
{
    class Order : INotifyPropertyChanged
    {
        private static int _counter = 1;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public int    Id          { get; }
        public int    DbId        { get; set; }
        public Room   Room        { get; }
        public string GuestName   { get; }
        public int    Days        { get; }
        public float  PricePerDay { get; }

        public DateTime  BookedCheckIn  { get; }
        public DateTime  BookedCheckOut { get; }
        public DateTime? CheckInDate    { get; set; }

        public string RoomDisplay => $"№ {Room.Id + 1}";

        public int ActualDays =>
            CheckInDate.HasValue
                ? Math.Max(1, (DateTime.Today - CheckInDate.Value.Date).Days + 1)
                : 0;

        public bool IsEarlyCheckOut =>
            CheckInDate.HasValue && ActualDays < Days;

        private float _totalPrice;
        public float TotalPrice
        {
            get => _totalPrice;
            set { _totalPrice = value; Notify(); Notify(nameof(FinalAmount)); }
        }

        private float _miniBarCharge;
        public float MiniBarCharge
        {
            get => _miniBarCharge;
            set { _miniBarCharge = value; Notify(); Notify(nameof(FinalAmount)); }
        }

        public float RefundAmount { get; set; }
        public float FinalAmount  => TotalPrice + MiniBarCharge;

        private IOrderState _state;
        public string StatusName => _state.StatusName;

        private readonly List<IOrderObserver> _observers = new();

        public void Subscribe(IOrderObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unsubscribe(IOrderObserver observer)
            => _observers.Remove(observer);

        private void NotifyObservers(string previousStatus)
        {
            foreach (var obs in _observers)
                obs.Update(this, previousStatus);
        }

        public Order(Room room, string guestName, float pricePerDay, int days, DateTime checkIn)
        {
            Id             = _counter++;
            Room           = room;
            GuestName      = guestName;
            Days           = days;
            PricePerDay    = pricePerDay;
            TotalPrice     = pricePerDay * days;
            BookedCheckIn  = checkIn;
            BookedCheckOut = checkIn.AddDays(days);
            _state         = new NewOrderState();
        }

        // Відновлення стану з БД без нотифікацій спостерігачів
        public void RestoreState(string status)
        {
            _state = status switch
            {
                "Оплачено"  => new PaidOrderState(),
                "Активне"   => new ActiveOrderState(),
                "Завершено" => new CompletedOrderState(),
                "Скасовано" => new CancelledOrderState(),
                _           => new NewOrderState()
            };
            Notify(nameof(StatusName));
        }

        public void SetState(IOrderState newState)
        {
            string prev = _state.StatusName;
            _state = newState;
            Notify(nameof(StatusName));
            NotifyObservers(prev);
        }

        public string? Pay()                             => _state.Pay(this);
        public string? Cancel()                          => _state.Cancel(this);
        public string? CheckIn()                         => _state.CheckIn(this);
        public string? CheckOut(float miniBarCharge = 0) => _state.CheckOut(this, miniBarCharge);
    }
}
