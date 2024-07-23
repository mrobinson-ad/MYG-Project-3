using Unity.Mathematics;

public static class Remap
{
    public static float RemapScore(float maxScore, float currentScore)
    {
        return math.remap(0, maxScore, 0, 1, currentScore);
    }
}
