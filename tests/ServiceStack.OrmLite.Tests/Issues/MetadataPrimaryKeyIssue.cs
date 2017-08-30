using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Expression;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class MetadataPrimaryKeyIssue : OrmLiteTestBase
    {
        [Test]
        public void Should_return_same_model_reference()
        {
            var model1 = typeof(LetterFrequency).GetModelMetadata();
            var model2 = typeof(LetterFrequency).GetModelMetadata();
            Assert.AreEqual(model1, model2);
        }

        [Test]
        public void Should_return_same_model_reference_multi_threaded()
        {
            Task<ModelDefinition> task1 = System.Threading.Tasks.Task.Run(() => typeof(LetterFrequency).GetModelMetadata());
            Task<ModelDefinition> task2 = System.Threading.Tasks.Task.Run(() => typeof(LetterFrequency).GetModelMetadata());
            System.Threading.Tasks.Task.WaitAll(task1, task2);

            Assert.AreEqual(task1.Result, task2.Result);
        }

        [Test]
        public void Should_generate_select_statement()
        {
            Assert.AreEqual(SelectStatement(), SelectStatement());
        }

        [Test]
        public void Should_generate_select_statement_multi_threaded()
        {
            Task<string> task1 = System.Threading.Tasks.Task.Run(() => SelectStatement());
            Task<string> task2 = System.Threading.Tasks.Task.Run(() => SelectStatement());
            System.Threading.Tasks.Task.WaitAll(task1, task2);

            Assert.AreEqual(task1.Result, task2.Result);
        }

        private string SelectStatement()
        {
            var pk = typeof(LetterFrequency).GetModelMetadata().PrimaryKey;
            using (var db = OpenDbConnection())
            {
                return db.From<LetterFrequency>().OrderByFields(pk).ToSelectStatement();
            }
        }
    }
}
