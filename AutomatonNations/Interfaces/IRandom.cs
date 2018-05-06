using System.Collections.Generic;

namespace AutomatonNations
{
    public interface IRandom
    {
        int[] IntegerSet(int maxVal, int count);

        double[] DoubleSet(double minVal, double maxVal, int count);

        T GetRandomElement<T>(IEnumerable<T> collection);

        T[] ShuffleElements<T>(IEnumerable<T> collection);
    }
}