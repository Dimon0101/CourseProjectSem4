using System.Collections.ObjectModel;
using CourseProjectSem4.Database;
using CourseProjectSem4.States;
using CourseProjectSem4.Templates;

namespace CourseProjectSem4.Commands
{
    class PayOrderCommand : IOrderCommand
    {
        private readonly Order _order;
        public string Name => $"Оплата замовлення #{_order.Id}";
        public PayOrderCommand(Order order) => _order = order;
        public string? Execute() => _order.Pay();
    }

    class CheckInCommand : IOrderCommand
    {
        private readonly Order _order;
        public string Name => $"Заселення гостя #{_order.Id} ({_order.GuestName})";
        public CheckInCommand(Order order) => _order = order;
        public string? Execute() => _order.CheckIn();
    }

    class CheckOutCommand : IOrderCommand
    {
        private readonly Order             _order;
        private readonly float             _miniBarCharge;
        private readonly CheckOutProcessor _processor;

        public string Name => $"Виселення гостя #{_order.Id} ({_order.GuestName})";

        public CheckOutCommand(Order order, float miniBarCharge, CheckOutProcessor processor)
        {
            _order         = order;
            _miniBarCharge = miniBarCharge;
            _processor     = processor;
        }

        public string? Execute() => _processor.ProcessCheckOut(_order, _miniBarCharge);
    }

    class CancelOrderCommand : IOrderCommand
    {
        private readonly Order _order;
        public string Name => $"Скасування замовлення #{_order.Id}";
        public CancelOrderCommand(Order order) => _order = order;
        public string? Execute() => _order.Cancel();
    }

    class CommandInvoker
    {
        public ObservableCollection<string> History { get; } = new();

        public string? Execute(IOrderCommand command)
        {
            string? error = command.Execute();
            string entry  = error == null
                ? $"✔  {command.Name}"
                : $"✘  {command.Name}  —  {error}";

            History.Insert(0, entry);
            HotelDb.AddLog("action", entry);
            return error;
        }
    }
}
