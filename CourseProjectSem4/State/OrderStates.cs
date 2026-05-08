using System;

namespace CourseProjectSem4.States
{
    class NewOrderState : IOrderState
    {
        public string StatusName => "Нове";

        public string? Pay(Order order)
        {
            order.SetState(new PaidOrderState());
            return null;
        }

        public string? Cancel(Order order)
        {
            order.SetState(new CancelledOrderState());
            return null;
        }

        public string? CheckIn(Order order) => "Спочатку необхідно оплатити замовлення.";
        public string? CheckOut(Order order, float miniBarCharge) => "Гість ще не заселився.";
    }

    class PaidOrderState : IOrderState
    {
        public string StatusName => "Оплачено";

        public string? Pay(Order order) => "Замовлення вже оплачено.";

        public string? Cancel(Order order)
        {
            order.RefundAmount = order.TotalPrice;
            order.SetState(new CancelledOrderState());
            return null;
        }

        public string? CheckIn(Order order)
        {
            order.CheckInDate = DateTime.Today;
            order.SetState(new ActiveOrderState());
            return null;
        }

        public string? CheckOut(Order order, float miniBarCharge) => "Гість ще не заселився.";
    }

    class ActiveOrderState : IOrderState
    {
        public string StatusName => "Активне";

        public string? Pay(Order order)    => "Замовлення вже активне та оплачене.";
        public string? Cancel(Order order) => "Неможливо скасувати активне замовлення. Спочатку виселіть гостя.";
        public string? CheckIn(Order order) => "Гість вже заселений.";

        public string? CheckOut(Order order, float miniBarCharge)
        {
            if (order.IsEarlyCheckOut)
            {
                order.RefundAmount = order.TotalPrice - order.PricePerDay * order.ActualDays;
                order.TotalPrice   = order.PricePerDay * order.ActualDays;
            }

            order.MiniBarCharge = miniBarCharge;
            order.SetState(new CompletedOrderState());
            return null;
        }
    }

    class CompletedOrderState : IOrderState
    {
        public string StatusName => "Завершено";

        public string? Pay(Order order)     => "Замовлення вже завершено.";
        public string? Cancel(Order order)  => "Замовлення вже завершено.";
        public string? CheckIn(Order order) => "Замовлення вже завершено.";
        public string? CheckOut(Order order, float miniBarCharge) => "Гість вже виселився.";
    }

    class CancelledOrderState : IOrderState
    {
        public string StatusName => "Скасовано";

        public string? Pay(Order order)     => "Замовлення скасовано.";
        public string? Cancel(Order order)  => "Замовлення вже скасовано.";
        public string? CheckIn(Order order) => "Замовлення скасовано.";
        public string? CheckOut(Order order, float miniBarCharge) => "Замовлення скасовано.";
    }
}
