using Z.EntityFramework.Plus;

namespace ConsoleJobScheduler.Core.Infra.Data
{
    public static class QueryableExtensions
    {
       public static async Task<PagedResult<TDestination>> List<TSource, TDestination>(
           this IQueryable<TSource> query,
           int pageSize,
           int page,
           Func<IQueryable<TSource>, IOrderedQueryable<TDestination>> selectFunc,
           Func<IQueryable<TSource>, IQueryable<TSource>>? filterFunc = null)
           {
               if (filterFunc != null)
               {
                   query = filterFunc(query);
               }

               var totalCount = query.DeferredCount().FutureValue();
               var itemsFutureValue = selectFunc(query).Skip((page - 1) * pageSize).Take(pageSize).Future();

               return new PagedResult<TDestination>(await itemsFutureValue.ToListAsync().ConfigureAwait(false), pageSize, page, await totalCount.ValueAsync().ConfigureAwait(false));
           }
    }
}