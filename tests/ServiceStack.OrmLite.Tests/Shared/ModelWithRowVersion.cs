using ServiceStack.DataAnnotations;

namespace ServiceStack.Common.Tests.Models
{
    public class ModelWithRowVersion
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Text { get; set; }

        [RowVersion]
        public long Version { get; set; }
    }
}