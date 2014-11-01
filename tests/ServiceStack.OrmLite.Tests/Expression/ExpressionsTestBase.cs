using System;
using System.Data;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests.Expression
{
    public class ExpressionsTestBase : OrmLiteTestBase
    {
        [SetUp]
        public void Setup()
        {
            OpenDbConnection().CreateTable<TestType>(true);
        }

        //Avoid painful refactor to change all tests to use a using pattern
        private IDbConnection db;

        public override IDbConnection OpenDbConnection()
        {
            try
            {
                if (db != null && db.State != ConnectionState.Open)
                    db = null;
            }
            catch (ObjectDisposedException) //PostgreSQL throws when trying to inspect db.State on a disposed connection. WHY???
            {
                db = null;
            }

            return db ?? (db = base.OpenDbConnection());
        }

        [TearDown]
        public void TearDown()
        {
            if (db == null)
                return;
            db.Dispose();
            db = null;
        }

        public T GetValue<T>(T item)
        {
            return item;
        }

        protected void EstablishContext(int numberOfRandomObjects)
        {
            EstablishContext(numberOfRandomObjects, null);
        }

        protected void EstablishContext(int numberOfRandomObjects, params TestType[] obj)
        {
            if (obj == null)
                obj = new TestType[0];

            using (var con = OpenDbConnection())
            {
                foreach (var t in obj)
                {
                    con.Insert(t);
                }

                var random = new Random((int)(DateTime.UtcNow.Ticks ^ (DateTime.UtcNow.Ticks >> 4)));
                for (var i = 0; i < numberOfRandomObjects; i++)
                {
                    TestType o = null;

                    while (o == null)
                    {
                        int intVal = random.Next();

                        o = new TestType
                        {
                            BoolColumn = random.Next() % 2 == 0,
                            IntColumn = intVal,
                            StringColumn = Guid.NewGuid().ToString()
                        };

                        if (obj.Any(x => x.IntColumn == intVal))
                            o = null;
                    }

                    con.Insert(o);
                }
            }
        }
    }
}