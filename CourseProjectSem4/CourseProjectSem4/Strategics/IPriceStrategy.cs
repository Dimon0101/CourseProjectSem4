using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProjectSem4.Strategics
{
    interface IPriceStrategy
    {
        float PriceCalculate(float price);
    }
    class NormalStrategy : IPriceStrategy
    {
        public float PriceCalculate(float price)
        {
            return price;
        }
    }

    class HolidayStrategy : IPriceStrategy
    {
        public float PriceCalculate(float price)
        {
            return price * 1.5f;
        }
    }

    class SalesStrategy : IPriceStrategy
    {
        public float PriceCalculate(float price)
        {
            return price * 0.75f;
        }
    }

    class TouristFullPeriodStrategy : IPriceStrategy
    {
        public float PriceCalculate(float price)
        {
            return price * 1.25f;
        }
    }
}
