namespace ConsoleJobScheduler.Service.Infrastructure.Data;

using System.Collections.ObjectModel;

public class PagedResult<TModel>
{
    public static readonly PagedResult<TModel> Empty = new(Enumerable.Empty<TModel>(), 0, 0, 0);
    
    public int Page { get; }

    public int TotalCount { get; }

    public IEnumerable<TModel> Items { get; }

    public int PageSize { get; }

    public int TotalPages { get; }

    public PagedResult(IEnumerable<TModel> items, int pageSize, int page, int totalCount)
    {
        Page = page;
        TotalCount = totalCount;
        Items = new ReadOnlyCollection<TModel>(new List<TModel>(items));
        PageSize = pageSize;
        TotalPages = Convert.ToInt32(Math.Ceiling(totalCount / (decimal) pageSize));
    }
}