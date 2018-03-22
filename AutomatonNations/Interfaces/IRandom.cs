namespace AutomatonNations
{
    public interface IRandom
    {
        int[] IntegerSet(int maxVal, int count);

        double[] DoubleSet(int count);
    }
}