namespace CourseProjectSem4.States
{
    interface IOrderState
    {
        string StatusName { get; }
        string? Pay(Order order);
        string? Cancel(Order order);
        string? CheckIn(Order order);
        string? CheckOut(Order order, float miniBarCharge);
    }
}
