TypesafeSQL
===========

Linq-like SQL query builder for use with micro ORMs.

The purpose of TypesafeSQL is to allow for generate SQL queries in typesafe way, thus it's not a real Linq provider to relational database. It provides familiar linq syntax to express queries, but doesn't implement IQueryable. 

Features
--------
* select, where, order by
* inner joins, 
* outer joins (not available with SQL-like syntax)
* group by
* count, sum, avg (only available in queries with group by)
* union, intersect, except
* almost all MS SQL functions
* subqueries (when defined outside a main query)
* easy integration with [Dapper.NET](http://code.google.com/p/dapper-dot-net/)
* mapping class names to table names and property names to column names with name resolvers

Supported databases
-------------------
For now, only MS SQL 2012 is supported. Future releases will support MySQL, PostgreSQL and SQLite.

Examples
--------

Simple SQL command generation:

    var builder = new QueryBuilder();
    var query = from u in builder.Table<User>() where u.LoginAttempts >= 3 select u;
    var sql = query.ToSql();

Retrieving data with Dapper:

    var connection = new SqlConnection(<Connection String>);
    var builder = new QueryBuilder();
    var users = connection.Query(from u in builder.Table<User>() where u.LoginAttempts >= 3 select u);
    
Inner joins:

    var builder = new QueryBuilder();
    var roles = from user in builder.Table<User>()
                join link in builder.Table<UserRoleLink>() on user.Id equals link.UserId
                join role in builder.Table<Role>() on link.RoleId equals role.Id
                select new { user.Login, Role = role.Name };

Outer joins:

    var builder = new QueryBuilder();
    var roles = builder.Table<User>()
        .LeftJoin(builder.Table<UserRoleLink>(), user => user.Id, link => link.UserId, (user, link) => new { user, link })
        .LeftJoin(builder.Table<Role>(), ul => ul.link.RoleId, role => role.Id, (ul, role) => new { ul, role })
        .Select(ulr => new { ulr.ul.user.Login, ulr.role.Name });

Group by:

    var builder = new QueryBuilder();
    var roleCounts = from l in builder.Table<UserRoleLink>()
                     group l by l.UserId into lg
                     select new { UserId = lg.Key, NumOfRoles = lg.Count(x => x.RoleId) };

Using name resolvers:

        public class PrefixBasedNameResolver : DefaultNameResolver
        {
            public override string ResolveTableName(Type modelClass)
            {
                return "tbl_" + base.ResolveTableName(modelClass);
            }
            
            public override string ResolveColumnName(System.Reflection.MemberInfo property)
            {
                return "col_" + base.ResolveColumnName(property);
            }
        }
        ...
        var builder = new QueryBuilder(new PrefixBasedNameResolver());
        ...
        
        
