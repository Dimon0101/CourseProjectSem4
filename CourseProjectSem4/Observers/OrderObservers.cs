using System;
using System.Collections.ObjectModel;
using CourseProjectSem4.Database;
using CourseProjectSem4.States;

namespace CourseProjectSem4.Observers
{
    class EventLogObserver : IOrderObserver
    {
        private readonly ObservableCollection<string> _log;

        public EventLogObserver(ObservableCollection<string> log) => _log = log;

        public void Update(Order order, string previousStatus)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            string msg  = $"[{time}]  Замовлення #{order.Id} ({order.GuestName})  {previousStatus}  →  {order.StatusName}";
            _log.Insert(0, msg);
            HotelDb.AddLog("event", msg);
        }
    }

    class BillingObserver : IOrderObserver
    {
        public float TotalRevenue   { get; private set; }
        public int   CompletedCount { get; private set; }

        public void Update(Order order, string previousStatus)
        {
            if (order.StatusName == "Завершено")
            {
                TotalRevenue   += order.FinalAmount;
                CompletedCount++;
            }
        }
    }
}
