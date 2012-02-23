using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests.UseCase
{
    [Alias("ImageBlobsDto")]
    public class ImageBlobDto
    {
        [AutoIncrement]
        public int Id { get; set; }

        public virtual Byte[] Image1 { get; set; }

        public virtual Byte[] Image2 { get; set; }

        public virtual Byte[] Image3 { get; set; }

        public virtual object[] Complex { get; set; } 
    }
}
