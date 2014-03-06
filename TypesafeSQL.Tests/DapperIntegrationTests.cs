using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesafeSQL;
using TypesafeSQL.Dapper;
using Dapper;
using System.Linq.Expressions;

namespace TypesafeSQL.Tests
{
    public class DapperIntegrationTests
    {
        private QueryBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new QueryBuilder();
            var dir = Directory.GetCurrentDirectory();
        }

        [Test]
        public void QueryOverModelExecutesCorreclty()
        {
            using (var connection = CreateConnection())
            {
                var users = connection.Query(from u in builder.Table<User>() where u.Login == "jacenty" select u);
                Assert.That(users.Count(), Is.EqualTo(1));
            }
        }

        private void AddTestData()
        {
            using (var connection = CreateConnection())
            {
                for (int i = 0; i < 97; i++)
                    connection.Execute("INSERT INTO Role (Name) VALUES (@Name)", new { Name = "Role_" + i });
            }
        }

        [Test]
        public void QueryOverModelExecutesFastEnough()
        {
            const int REPETITIONS = 1000;
            AddTestData();
            var count = 0;
            using (var connection = CreateConnection())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var executor = new QueryExecutor(connection.ConnectionString);
                for (var i = 0; i < REPETITIONS; i++)
                    count = executor.LoadObjects<Role>(@"select * from Role", new Dictionary<string, object>()).ToList().Count;
                sw.Stop();
                Console.WriteLine("ELEMENTS: {0}", count);
                Console.WriteLine("SQL + EXECUTOR: {0}", sw.Elapsed);
                sw.Reset();
                sw.Start();
                for (var i = 0; i < REPETITIONS; i++)
                    count = connection.Query<User>(@"select * from Role").Count();
                sw.Stop();
                Console.WriteLine("ELEMENTS: {0}", count);
                Console.WriteLine("SQL + DAPPER:  {0}", sw.Elapsed);
                sw.Reset();
                sw.Start();
                for (var i = 0; i < REPETITIONS; i++)
                {
                    Expression<Func<IQuery<Role>>> roles = () => from r in builder.Table<Role>() select r;
                    roles.ToString();
                    count = connection.Query<User>(@"select * from Role").Count();
                }
                sw.Stop();
                Console.WriteLine("ELEMENTS: {0}", count);
                Console.WriteLine("CACHED LINQ + DAPPER:  {0}", sw.Elapsed);
                sw.Reset();
                sw.Start();
                for (var i = 0; i < REPETITIONS; i++)
                    count = connection.Query(from r in builder.Table<Role>() select r).Count();
                sw.Stop();
                Console.WriteLine("ELEMENTS: {0}", count);
                Console.WriteLine("LINQ + DAPPER: {0}", sw.Elapsed);
            }

        }

        private IDbConnection CreateConnection()
        {
            var dir = Directory.GetCurrentDirectory();
            var connection = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=" + dir + @"\App_Data\TestData.mdf;Integrated Security=True;Connect Timeout=30");
            connection.Open();
            return connection;
        }
    }
}
