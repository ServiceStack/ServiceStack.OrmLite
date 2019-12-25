using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests.Interceptors
{
    [Alias("emails")]
    public partial class EmailAddressesPoco
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Domain { get; set; }
    }
}
