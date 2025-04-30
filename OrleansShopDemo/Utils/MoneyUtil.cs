using System;

namespace OrleansShopDemo.Utils 
{
    public static class MoneyUtil
    {
        public static decimal CombinePrice(long units, int nanos)
        {
            return units + nanos / 1_000_000_000m;
        }

        public static Tuple<long, int> SplitPrice(decimal price)
        {
            long units = (long)price;
            int nanos = (int)((price - units) * 1_000_000_000m);

            return Tuple.Create(units, nanos);
        }
    }
}
