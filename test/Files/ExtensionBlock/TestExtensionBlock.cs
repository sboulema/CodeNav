namespace CodeNav.Test.Files.ExtensionBlock;

public static class MyExtensions
{
    public static IEnumerable<int> ValuesLessThan(this IEnumerable<int> source, int threshold)
            => source.Where(x => x < threshold);

    extension(IEnumerable<int> source)
    {
        public IEnumerable<int> ValuesGreaterThan(int threshold)
            => source.Where(x => x > threshold);

        public IEnumerable<int> ValuesGreaterThanZero
            => source.ValuesGreaterThan(0);
    }
}
