using System.Collections.Generic;

namespace Autyan.Identity.Core.Data
{
    public class PagedResult<TEntity>
    {
        public IEnumerable<TEntity> Results { get; set; }

        public int TotalCount { get; set; }
    }
}
