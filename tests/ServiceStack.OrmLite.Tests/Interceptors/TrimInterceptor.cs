using System.Data;
using System.Threading.Tasks;
using ServiceStack.OrmLite.Interceptors;

namespace ServiceStack.OrmLite.Tests.Interceptors
{
    public class TrimInterceptor : IEntityInterceptor<EmailAddressesPoco>
    {
        public string Name => "InterceptedPocoEntity";

        public Task OnInsertAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
        {
            entity.Email = entity.Email.Trim();
            return Task.CompletedTask;
        }

        public Task OnUpdateAsync(IDbCommand dbCmd, EmailAddressesPoco entity)
        {
            entity.Email = entity.Email.Trim();
            return Task.CompletedTask;
        }
    }
}