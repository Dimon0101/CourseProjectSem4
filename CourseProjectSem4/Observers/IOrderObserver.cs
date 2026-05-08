using CourseProjectSem4.States;

namespace CourseProjectSem4.Observers
{
    interface IOrderObserver
    {
        void Update(Order order, string previousStatus);
    }
}
