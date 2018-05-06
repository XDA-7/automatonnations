using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatonNations
{
    public class RandomWrapper : IRandom
    {
        private Random _random = new Random();

        public int[] IntegerSet(int maxVal, int count)
        {
            var result = new int[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = _random.Next(maxVal);
            }

            return result;
        }

        public double[] DoubleSet(double minVal, double maxVal, int count)
        {
            var result = new double[count];
            var range = maxVal - minVal;
            for (var i = 0; i < count; i++)
            {
                var next = _random.NextDouble();
                result[i] = minVal + (next * range);
            }

            return result;
        }

        public T GetRandomElement<T>(IEnumerable<T> collection)
        {
            if (!collection.Any())
            {
                return default(T);
            }
            else
            {
                var collectionArray = collection.ToArray();
                var elementIndex = (int)Math.Floor(_random.NextDouble() * collectionArray.Length);
                return collectionArray[elementIndex];
            }
        }

        public T[] ShuffleElements<T>(IEnumerable<T> collection)
        {
            var remainingElements = collection.ToList();
            var result = new T[remainingElements.Count];
            for (var i = 0; i < result.Length; i++)
            {
                var elementIndex = (int)Math.Floor(_random.NextDouble() * remainingElements.Count);
                var element = remainingElements[elementIndex];
                result[i] = element;
                remainingElements.Remove(element);
            }

            return result;
        }
    }
}