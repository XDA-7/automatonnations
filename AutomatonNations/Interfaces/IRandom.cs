namespace AutomatonNations
{
    public interface IRandom
    {
        int[] IntegerSet(int maxVal, int count);

        double[] DoubleSet(double minVal, double maxVal, int count);
    }
}