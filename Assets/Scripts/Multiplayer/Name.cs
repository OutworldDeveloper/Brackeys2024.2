public static class Name
{

    private static readonly string[] _potentialNames = new string[]
    {
        "George Floyed",
        "Adolf Hitler",
        "Vladimir Putin",
        "Marilyn Monroe",
        "Slark Anton",
        "Johnny Depp",
        "Mama Prishla"
    };

    public static readonly string Mine = GetRandom();

    public static string GetRandom()
    {
        return _potentialNames[Randomize.Index(_potentialNames.Length)];
    }

}
