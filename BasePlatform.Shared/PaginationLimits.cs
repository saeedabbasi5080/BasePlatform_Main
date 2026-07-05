namespace BasePlatform.Shared;

public static class PaginationLimits
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static int NormalizePage(int page) => page < 1 ? 1 : page;

    public static int NormalizePageSize(int pageSize) =>
        pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };
}
