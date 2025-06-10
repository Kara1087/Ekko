// Scripts/Core/LandingUtils.cs
public static class LandingUtils
{
    public static bool IsHeavyImpact(float force, LandingType type, float slamThreshold = 25f)
    {
        return type == LandingType.Slam || force >= slamThreshold;
    }
}
