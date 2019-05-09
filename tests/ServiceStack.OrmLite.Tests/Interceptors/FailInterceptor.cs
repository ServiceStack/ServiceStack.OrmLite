using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite.Interceptors;

namespace ServiceStack.OrmLite.Tests.Interceptors
{
    public class FailInterceptor : IEntityInterceptor<EmailAddressesPoco>
    {
        public string Name => "Fail";

        public Task OnInsertAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
        {
            throw new NotImplementedException();
        }

        public Task OnUpdateAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
        {
            throw new NotImplementedException();
        }
    }
}
