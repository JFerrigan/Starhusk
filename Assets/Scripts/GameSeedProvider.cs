using System.Threading;

public static class GameSeedProvider
{
    private static int nextSeed = 1107;

    public static int NextSeed()
    {
        return Interlocked.Increment(ref nextSeed);
    }
}
