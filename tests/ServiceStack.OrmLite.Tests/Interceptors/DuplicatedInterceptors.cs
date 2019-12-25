using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite.Interceptors;

namespace ServiceStack.OrmLite.Tests.Interceptors
{
    public abstract class DuplicatedInterceptors
    {
        public class InterceptorCamelCase : IEntityInterceptor<EmailAddressesPoco>
        {
            public string Name => "NAME";
            public Task OnInsertAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
            {
                throw new NotImplementedException();
            }

            public Task OnUpdateAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
            {
                throw new NotImplementedException();
            }
        }

        public class Interceptor1 : IEntityInterceptor<EmailAddressesPoco>
        {
            public string Name => "name";
            public Task OnInsertAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
            {
                throw new NotImplementedException();
            }

            public Task OnUpdateAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
            {
                throw new NotImplementedException();
            }
        }

        public class Interceptor2 : IEntityInterceptor<EmailAddressesPoco>
        {
            public string Name => "name";
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
}
