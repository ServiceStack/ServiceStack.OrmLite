using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests.Async;
using ServiceStack.OrmLite.Tests.Interceptors;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixtureOrmLite]
    public class InterceptorsTests : OrmLiteProvidersTestBase
    {
        public InterceptorsTests(DialectContext context) : base(context)
        {
        }

        [Test]
        public async Task Can_Insert_Calling_Interceptor()
        {
            OrmLiteConfig.RemoveAllInterceptors();

            using (var db = OpenDbConnection())
            {
                OrmLiteConfig.AddInterceptor(new TrimInterceptor());

                db.DropAndCreateTable<EmailAddressesPoco>();
                await db.InsertAsync(new EmailAddressesPoco
                {
                    Email = "test@servicestack.com       "
                });

                var inserted = (await db.SelectAsync<EmailAddressesPoco>()).FirstOrDefault();

                Assert.NotNull(inserted);
                Assert.AreEqual("test@servicestack.com", inserted.Email);
            }
        }

        [Test]
        public async Task Can_Use_Multiple_Interceptors()
        {
            OrmLiteConfig.RemoveAllInterceptors();

            using (var db = OpenDbConnection())
            {
                OrmLiteConfig.AddInterceptor(new TrimInterceptor());
                OrmLiteConfig.AddInterceptor(new CompleteDomainInterceptor());

                db.DropAndCreateTable<EmailAddressesPoco>();
                await db.InsertAsync(new EmailAddressesPoco
                {
                    Email = "test@servicestack.com       "
                });

                var inserted = (await db.SelectAsync<EmailAddressesPoco>()).FirstOrDefault();

                Assert.NotNull(inserted);
                Assert.AreEqual("test@servicestack.com", inserted.Email);
                Assert.AreEqual("servicestack.com", inserted.Domain);
            }
        }

        [Test]
        public void Interceptors_Are_Throwing_Exceptions_For_Insert()
        {
            OrmLiteConfig.RemoveAllInterceptors();

            using (var db = OpenDbConnection())
            {
                OrmLiteConfig.AddInterceptor(new FailInterceptor());

                db.DropAndCreateTable<EmailAddressesPoco>();

                Assert.ThrowsAsync<NotImplementedException>(async () =>
                    await db.InsertAsync(new EmailAddressesPoco
                    {
                        Email = "test@servicestack.com"
                    }));
            }
        }

        [Test]
        public async Task Can_Update_Calling_Interceptor()
        {
            OrmLiteConfig.RemoveAllInterceptors();

            using (var db = OpenDbConnection())
            {
                OrmLiteConfig.AddInterceptor(new TrimInterceptor());

                db.DropAndCreateTable<EmailAddressesPoco>();

                await db.InsertAsync(new EmailAddressesPoco
                {
                    Email = "oldemail@servicestack.com"
                });

                await db.UpdateAsync(new EmailAddressesPoco
                {
                    Email = "test@servicestack.com       "
                });

                var inserted = (await db.SelectAsync<EmailAddressesPoco>()).FirstOrDefault();

                Assert.NotNull(inserted);
                Assert.AreEqual("test@servicestack.com", inserted.Email);
            }
        }

        [Test]
        public void Duplicated_Interceptors_Are_Ignored()
        {
            OrmLiteConfig.RemoveAllInterceptors();

            var interceptor = new DuplicatedInterceptors.Interceptor1();
            var duplicated = new DuplicatedInterceptors.Interceptor2();

            OrmLiteConfig.AddInterceptor(interceptor);
            OrmLiteConfig.AddInterceptor(duplicated);

            // the second one should be ignored as the name is the same
            Assert.AreEqual(interceptor.Name, duplicated.Name);
            Assert.AreEqual(1, OrmLiteConfig.RegisteredInterceptors.Count);
        }

        [Test]
        public void Interceptor_Name_Is_Case_Sensitive()
        {
            OrmLiteConfig.RemoveAllInterceptors();

            var interceptor = new DuplicatedInterceptors.Interceptor1();
            var duplicated = new DuplicatedInterceptors.InterceptorCamelCase();

            OrmLiteConfig.AddInterceptor(interceptor);
            OrmLiteConfig.AddInterceptor(duplicated);

            // the second one should be ignored as the name is the same
            Assert.AreEqual(interceptor.Name.ToLower(), duplicated.Name.ToLower());
            Assert.True(interceptor.Name != duplicated.Name);
            Assert.AreEqual(2, OrmLiteConfig.RegisteredInterceptors.Count);
        }

        [Test]
        public async Task Filters_And_Interceptors_Are_Compatible()
        {
            OrmLiteConfig.RemoveAllInterceptors();

            using (var db = OpenDbConnection())
            {
                OrmLiteConfig.AddInterceptor(new TrimInterceptor());
                OrmLiteConfig.InsertFilter = (command, o) =>
                {
                    if (o is EmailAddressesPoco entity)
                    {
                        entity.Domain = "ormlite.com";
                    }
                };

                db.DropAndCreateTable<EmailAddressesPoco>();
                await db.InsertAsync(new EmailAddressesPoco
                {
                    Email = "test@servicestack.com       "
                });

                var inserted = (await db.SelectAsync<EmailAddressesPoco>()).FirstOrDefault();
                Assert.NotNull(inserted);
                Assert.AreEqual("test@servicestack.com", inserted.Email);
                Assert.AreEqual("ormlite.com", inserted.Domain);
            }
        }
    }
}
