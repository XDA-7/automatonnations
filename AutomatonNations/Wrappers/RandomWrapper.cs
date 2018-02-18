using System;

namespace AutomatonNations
{
    public class RandomWrapper : IRandom
    {
        private Random _random = new Random();

        public int[] NextSet(int maxVal, int count)
        {
            var result = new int[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = _random.Next(maxVal);
            }

            return result;
        }
    }
}