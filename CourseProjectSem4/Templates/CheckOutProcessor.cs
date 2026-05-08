using CourseProjectSem4.States;

namespace CourseProjectSem4.Templates
{
    abstract class CheckOutProcessor
    {
        public string? ProcessCheckOut(Order order, float miniBarCharge)
        {
            InspectRoom(order);
            ApplyPenalty(order);
            ApplyMiniBar(order, miniBarCharge);
            string? error = order.CheckOut(miniBarCharge);
            if (error != null) return error;
            GenerateReceipt(order);
            return null;
        }

        protected abstract void InspectRoom(Order order);
        protected abstract void GenerateReceipt(Order order);
        protected virtual  void ApplyPenalty(Order order) { }
        protected virtual  void ApplyMiniBar(Order order, float charge) { }

        protected static string EarlyCheckOutNote(Order order)
        {
            if (order.RefundAmount <= 0) return "";
            return
                $"       ⚠ Дострокове виселення!\n" +
                $"       Заброньовано діб:  {order.Days}\n" +
                $"       Фактично прожито:  {order.ActualDays}\n" +
                $"       Повернення:        {order.RefundAmount} грн\n";
        }
    }

    class StandardCheckOutProcessor : CheckOutProcessor
    {
        public string LastReceipt { get; private set; } = "";

        protected override void InspectRoom(Order order)
            => LastReceipt = $"[Перевірка] Номер {order.Room.Id} — стан задовільний.\n";

        protected override void ApplyMiniBar(Order order, float charge)
        {
            if (charge > 0) LastReceipt += $"[Міні-бар]  Нараховано: {charge} грн\n";
        }

        protected override void GenerateReceipt(Order order)
        {
            LastReceipt +=
                $"[Чек]  Гість:         {order.GuestName}\n" +
                $"       Номер:         {order.Room.Id}\n" +
                $"       Ціна за ніч:   {order.PricePerDay} грн\n" +
                $"       Кількість діб: {order.ActualDays}\n" +
                EarlyCheckOutNote(order) +
                $"       Проживання:    {order.TotalPrice} грн\n" +
                $"       Міні-бар:      {order.MiniBarCharge} грн\n" +
                $"       РАЗОМ:         {order.FinalAmount} грн\n" +
                $"       Оплата прийнята. Дякуємо!";
        }
    }

    class DeluxeCheckOutProcessor : CheckOutProcessor
    {
        public string LastReceipt { get; private set; } = "";

        protected override void InspectRoom(Order order)
            => LastReceipt = $"[Делюкс-перевірка] Номер {order.Room.Id} — огляд виконано.\n";

        protected override void ApplyMiniBar(Order order, float charge)
        {
            if (charge > 0) LastReceipt += $"[Міні-бар]  Нараховано: {charge} грн\n";
        }

        protected override void GenerateReceipt(Order order)
        {
            LastReceipt +=
                $"[Делюкс Чек]  Гість:         {order.GuestName}\n" +
                $"              Номер:         {order.Room.Id} (Делюкс)\n" +
                $"              Ціна за ніч:   {order.PricePerDay} грн\n" +
                $"              Кількість діб: {order.ActualDays}\n" +
                EarlyCheckOutNote(order) +
                $"              Проживання:    {order.TotalPrice} грн\n" +
                $"              Міні-бар:      {order.MiniBarCharge} грн\n" +
                $"              РАЗОМ:         {order.FinalAmount} грн\n" +
                $"              Оплата прийнята. До зустрічі!";
        }
    }
}
