namespace CourseProjectSem4.Commands
{
    interface IOrderCommand
    {
        string Name    { get; }
        string? Execute();
    }
}
