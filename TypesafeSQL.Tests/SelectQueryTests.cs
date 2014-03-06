using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypesafeSQL;

namespace TypesafeSQL.Tests
{
    [TestFixture]
    public class SelectQueryTests
    {
        private SelectQueryData CreateQueryData<TModel>()
        {
            var resolver = new DefaultNameResolver();
            return new SelectQueryData(
                    new SqlCommandBuilder(resolver), 
                    new ModelQuerySource(typeof(TModel), resolver));
        }

        private IQuery<TModel> CreateQuery<TModel>(SelectQueryData data)
        {
            var resolver = new DefaultNameResolver();
            var factory = new DefaultQueryDataFactory(resolver);
            return new SelectQuery<TModel>(data, factory);
        }

        [Test]
        public void WhereSetsQueryDataWhereClauseProperty()
        {
            var data = CreateQueryData<User>();
            var query = CreateQuery<User>(data);
            query.Where(u => u.Login.StartsWith("jac"));
            Assert.That(data.WhereClause, Is.Not.Null);
        }

        [Test]
        public void OrderByUpdatesQueryDataOrderByPropertiesProperty()
        {
            var data = CreateQueryData<User>();
            var query = CreateQuery<User>(data);
            query.OrderBy(u => u.LastName);
            Assert.That(data.OrderByProperties.Where(t => t.Item2), Is.Not.Empty);
        }

        [Test]
        public void OrderByDescendingUpdatesQueryDataOrderByPropertiesProperty()
        {
            var data = CreateQueryData<User>();
            var query = CreateQuery<User>(data);
            query.OrderByDescending(u => u.LastName);
            Assert.That(data.OrderByProperties.Where(t => !t.Item2), Is.Not.Empty);
        }

        [Test]
        public void SelectSetsQueryDataSelectClauseProperty()
        {
            var data = CreateQueryData<User>();
            var query = CreateQuery<User>(data);
            query.Select(u => u.Email);
            Assert.That(data.SelectClause, Is.Not.Null);
        }

    }
}
