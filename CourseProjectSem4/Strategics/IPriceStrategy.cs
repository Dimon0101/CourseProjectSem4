namespace CourseProjectSem4.Strategics
{
    interface IPriceStrategy
    {
        float PriceCalculate(float price);
    }

    class NormalStrategy : IPriceStrategy
    {
        public float PriceCalculate(float price) => price;
    }

    class HolidayStrategy : IPriceStrategy
    {
        private readonly float _percent;
        public HolidayStrategy(float percent) => _percent = percent;
        public float PriceCalculate(float price) => price + price * _percent;
    }

    class SalesStrategy : IPriceStrategy
    {
        private readonly float _percent;
        public SalesStrategy(float percent) => _percent = percent;
        public float PriceCalculate(float price) => price + price * _percent;
    }

    class TouristFullPeriodStrategy : IPriceStrategy
    {
        private readonly float _percent;
        public TouristFullPeriodStrategy(float percent) => _percent = percent;
        public float PriceCalculate(float price) => price + price * _percent;
    }
}
