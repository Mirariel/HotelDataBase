namespace DataBase.Extensions
{
    public class SortingDictionary<T> : Dictionary<string, Func<IQueryable<T>, IQueryable<T>>>
    {
        private Func<IQueryable<T>, IQueryable<T>>? _defaultSort;

        public void SetDefaultSort(Func<IQueryable<T>, IQueryable<T>> defaultSort)
        {
            _defaultSort = defaultSort;
        }

        public IQueryable<T> ApplySorting(IQueryable<T> query, string sortOrder)
        {
            return TryGetValue(sortOrder ?? string.Empty, out var sortFunc)
                ? sortFunc(query)
                : _defaultSort?.Invoke(query) ?? query;
        }
    }
}
