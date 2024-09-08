using UnityEngine;

public static class Randomize
{

    public static int Index(int from, int length)
    {
        return Random.Range(from, length);
    }

    public static int Index(int length)
    {
        return Index(0, length);
    }

    public static int Int(int minInclusive, int maxInclusive)
    {
        return Random.Range(minInclusive, maxInclusive + 1);
    }

    public static int Int(MinMax<int> minMax)
    {
        return Int(minMax.Min, minMax.Max);
    }

    public static float Float(float minInclusive, float maxInclusive)
    {
        return Random.Range(minInclusive, maxInclusive);
    }

    public static float Float(MinMax<float> minMax)
    {
        return Float(minMax.Min, minMax.Max);
    }

    public static bool Bool()
    {
        return Int(0, 1) == 0;
    }

    public static int Sign()
    {
        return Bool() ? 1 : -1;
    }

    public static int Sign(int original)
    {
        return original * Sign();
    }

    public static float Sign(float original)
    {
        return original * Sign();
    }

    public static bool Chance(int chance)
    {
        return Int(1, chance) == chance;
    }

}
