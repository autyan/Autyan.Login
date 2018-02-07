using System;

namespace Autyan.Identity.Core.Data
{
    public class BaseQuery
    {
        public long? Id { get; set; }

        public long[] IdRange { get; set; }

        public long? IdFrom { get; set; }

        public long? IdTo { get; set; }

        public DateTime? CreatedAtFrom { get; set; }

        public DateTime? CreatedAtTo { get; set; }

        public DateTime? LastModifiedAtFrom { get; set; }

        public DateTime? LastModifiedAtTo { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }
    }

    public class BaseQuery<TEntity> : BaseQuery where TEntity : BaseEntity
    {
    }
}
