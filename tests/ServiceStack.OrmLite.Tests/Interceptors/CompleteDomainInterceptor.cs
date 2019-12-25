using System.Data;
using System.Threading.Tasks;
using ServiceStack.OrmLite.Interceptors;

namespace ServiceStack.OrmLite.Tests.Interceptors
{
    public class CompleteDomainInterceptor : IEntityInterceptor<EmailAddressesPoco>
    {
        public string Name => "CompleteValueInterceptor";

        public Task OnInsertAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
        {
            entity.Domain = entity.Email?.Split('@')[1];
            return Task.CompletedTask;
        }

        public Task OnUpdateAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
        {
            return Task.CompletedTask;
        }
    }
}