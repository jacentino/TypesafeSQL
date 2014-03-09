using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypesafeSQL;

namespace TypesafeSQL.Tests
{
    [TestFixture]
    public class SqlCommandBuilderTests
    {
        private class EmailInfo
        {
            public string UserName { get; set; }
            public string Email { get; set; }
        }

        private class RoleInfo
        {
            public string Login { get; set; }
            public string RoleName { get; set; }
        }

        private class PrefixBasedNameResolver : DefaultNameResolver
        {
            public override string ResolveTableName(Type modelClass)
            {
                return "TBL_" + base.ResolveTableName(modelClass);
            }

            public override string ResolveColumnName(System.Reflection.MemberInfo property)
            {
                return "col_" + base.ResolveColumnName(property);
            }
        }

        [Test]
        public void GetSqlCommandChecksParameterForNull()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            Assert.Throws<ArgumentNullException>(() => builder.GetSelectCommand(null));
        }

        [Test]
        public void GetSqlCommandGeneratesSimplestSelectIfNoAdditionalArgumentsPassed()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            var command = builder.GetSelectCommand(new SelectQueryData(builder, source));
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User]"));
        }

        [Test]
        public void GetSqlCommandGeneratesSelectWithWhereClauseIfWhereClauseSpecified()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => user.Login == user.FirstName;
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);     
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE ([user].[Login]) = ([user].[FirstName])"));
        }

        [Test]
        public void GetSqlCommandCreatesParameterWhenConstantIsUsedInExpression()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => user.Login == "jacenty";
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE ([user].[Login]) = (@p0)"));
            Assert.That(command.Parameters["@p0"], Is.EqualTo("jacenty"));
        }

        [Test]
        public void GetSqlCommandCreatesParameterWhenVariableFromEnclosingScopeIsUsedInExpression()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            var jacenty = "jacenty";
            Expression<Predicate<User>> whereClause = user => user.Login == jacenty;
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE ([user].[Login]) = (@p0)"));
            Assert.That(command.Parameters["@p0"], Is.EqualTo("jacenty"));
        }

        [Test]
        public void GetSqlCommandStartsWithMethodIsTranslatedToLikeOperator()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => user.Login.StartsWith("jac");
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE ([user].[Login]) LIKE ((@p0) + '%')"));
        }

        [Test]
        public void GetSqlCommandContainsMethodIsTranslatedToLikeOperator()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => user.Login.Contains("cent");
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE ([user].[Login]) LIKE ('%' + (@p0) + '%')"));
        }

        [Test]
        public void GetSqlCommandSubstringMethodIsTranslatedToSubstring()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => user.Login.Substring(1, 3) == "ace";
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE (SUBSTRING([user].[Login], @p0 + 1, @p1)) = (@p2)"));
        }

        [Test]
        public void GetSqlCommandStringLengthIsTranslatedToLen()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => user.Login.Length > 3;
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE (LEN([user].[Login])) > (@p0)"));
        }

        [Test]
        public void GetSqlCommandAndOperatorIsTranslatedCorrectly()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => user.FirstName == "Jacek" && user.LastName == "Hełka";
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(
                command.Command, 
                Is.EqualTo("SELECT * FROM [User] [user] WHERE (([user].[FirstName]) = (@p0)) AND (([user].[LastName]) = (@p1))"));
        }

        [Test]
        public void GetSqlCommandTranslatesNotOperatorCorrectly()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => !(user.FirstName == "Jacek");
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE NOT (([user].[FirstName]) = (@p0))"));
        }

        [Test]
        public void GetSqlCommandTranslatesNegateOperatorCorrectly()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => -user.Id < 10;
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE (- ([user].[Id])) < (@p0)"));
        }

        [Test]
        public void GetSqlCommandTranslatesConditionalOperatorCorrectly()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Predicate<User>> whereClause = user => (user.Login != null ? user.Login : user.FirstName).StartsWith("jac");
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(
                command.Command, 
                Is.EqualTo("SELECT * FROM [User] [user] WHERE (IIF(([user].[Login]) IS NOT (NULL), [user].[Login], [user].[FirstName])) LIKE ((@p0) + '%')"));
        }

        [Test]
        public void GetSqlCommandGeneratesSelectClauseWithOneColumnIfSingleExpressionSpecified()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Func<User, object>> selectClause = user => user.Login;
            var command = builder.GetSelectCommand(new SelectQueryData(builder, source)
            {
                SelectClause = selectClause
            });
            Assert.That(command.Command, Is.EqualTo("SELECT [user].[Login] FROM [User] [user]"));
        }

        [Test]
        public void GetSqlCommandGeneratesSelectClauseWithManyColumnsIfConstuctorWithMemberInitializationPassed()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Func<User, object>> selectClause = user => new EmailInfo 
            { 
                UserName = user.FirstName + " " + user.LastName, 
                Email = user.Email 
            };
            var command = builder.GetSelectCommand(new SelectQueryData(builder, source)
            {
                SelectClause = selectClause
            });
            Assert.That(
                command.Command, 
                Is.EqualTo("SELECT (([user].[FirstName]) + (@p0)) + ([user].[LastName]) AS [UserName], [user].[Email] AS [Email] FROM [User] [user]"));
        }

        [Test]
        public void GetSqlCommandGeneratesSelectClauseWithManyColumnsIfConstuctorOfDynamicTypeWithMemberInitializationPassed()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Func<User, object>> selectClause = user => new
            {
                UserName = user.FirstName + " " + user.LastName,
                Email = user.Email
            };
            var command = builder.GetSelectCommand(new SelectQueryData(builder, source)
            {
                SelectClause = selectClause
            });
            Assert.That(
                command.Command,
                Is.EqualTo("SELECT (([user].[FirstName]) + (@p0)) + ([user].[LastName]) AS [UserName], [user].[Email] AS [Email] FROM [User] [user]"));
        }

        [Test]
        public void GetSqlCommandGeneratesOrderByIfOrderByPropertyAdded()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());        
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            var data = new SelectQueryData(builder, source);
            Expression<Func<User, object>> orderSpec = user => user.Login;
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec, true));
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] ORDER BY [user].[Login]"));
        }

        [Test]
        public void GetSqlCommandGeneratesOrderByDescendingIfOrderByPropertyWithDescendingAdded()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            var data = new SelectQueryData(builder, source);
            Expression<Func<User, object>> orderSpec = user => user.Login;
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec, false));
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] ORDER BY [user].[Login] DESC"));
        }

        [Test]
        public void GetSqlCommandGeneratesOrderByIfOrderByWhenManyOrderByPropertiesAdded()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            var data = new SelectQueryData(builder, source);
            Expression<Func<User, object>> orderSpec1 = user => user.LastName;
            Expression<Func<User, object>> orderSpec2 = user => user.FirstName;
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec1, true));
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec2, true));
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] ORDER BY [user].[LastName], [user].[FirstName]"));
        }

        [Test]
        public void GetSqlCommandWhenSelectClauseIsPresentTableAliasIsTakenFromIt()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Func<User, object>> selectClause = user => user.Email;
            var command = builder.GetSelectCommand(new SelectQueryData(builder, source)
            {
                SelectClause = selectClause
            });
            Assert.That(command.Command, Is.EqualTo("SELECT [user].[Email] FROM [User] [user]"));
        }

        [Test]
        public void GetSqlCommandWhenWhereClauseIsPresentTableAliasIsTakenFromIt()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Func<User, bool>> whereClause = user => user.Login == "jacenty";
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE ([user].[Login]) = (@p0)"));
        }

        [Test]
        public void GetSqlCommandGeneratesOffsetWhenSkipUsed()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            var data = new SelectQueryData(builder, source)
            {
                SkipRows = 5
            };
            Expression<Func<User, object>> orderSpec1 = user => user.LastName;
            Expression<Func<User, object>> orderSpec2 = user => user.FirstName;
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec1, true));
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec2, true));
            var command = builder.GetSelectCommand(data);
            Assert.That(
                command.Command, 
                Is.EqualTo("SELECT * FROM [User] [user] ORDER BY [user].[LastName], [user].[FirstName] OFFSET 5 ROWS"));
        }

        [Test]
        public void GetSqlCommandGeneratesOffsetAndFetchWhenTakeUsed()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            var data = new SelectQueryData(builder, source)
            {
                TakeRows = 20
            };
            Expression<Func<User, object>> orderSpec1 = user => user.LastName;
            Expression<Func<User, object>> orderSpec2 = user => user.FirstName;
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec1, true));
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec2, true));
            var command = builder.GetSelectCommand(data);
            Assert.That(
                command.Command, 
                Is.EqualTo("SELECT * FROM [User] [user] ORDER BY [user].[LastName], [user].[FirstName] OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY"));
        }

        [Test]
        public void GetSqlCommandGeneratesOffsetAndFetchWhenSkipAndTakeUsed()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            var data = new SelectQueryData(builder, source)
            {
                SkipRows = 10,
                TakeRows = 20
            };
            Expression<Func<User, object>> orderSpec1 = user => user.LastName;
            Expression<Func<User, object>> orderSpec2 = user => user.FirstName;
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec1, true));
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec2, true));
            var command = builder.GetSelectCommand(data);
            Assert.That(
                command.Command,
                Is.EqualTo("SELECT * FROM [User] [user] ORDER BY [user].[LastName], [user].[FirstName] OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY"));
        }

        [Test]
        public void GetSqlCommandDateTimeConstructorIsTranslatedIntoDateTimeFromParts()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Func<User, bool>> whereClause = user => user.BirthDate == new DateTime(1968, 06, 02);
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE ([user].[BirthDate]) = (DATETIMEFROMPARTS(@p0, @p1, @p2, 0, 0, 0, 0))"));
        }

        [Test]
        public void GetSqlCommandTranslatesSubtractDaysToDateDiffDays()
        {
            var builder = new SqlCommandBuilder(new DefaultNameResolver());
            var source = new ModelQuerySource(typeof(User), new DefaultNameResolver());
            Expression<Func<User, bool>> whereClause = user => DateTime.Today.Subtract(user.BirthDate).Days > 100;
            var data = new SelectQueryData(builder, source);
            data.WhereClauses.Add(whereClause);
            var command = builder.GetSelectCommand(data);
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [user] WHERE (DATEDIFF(day, [user].[BirthDate], GETDATE())) > (@p0)"));
        }

        [Test]
        public void GetSqlCommandJoinIsTranslatedToSqlJoin()
        {
            var builder = new QueryBuilder();
            var roles = from user in builder.Table<User>()
                        join link in builder.Table<UserRoleLink>() on user.Id equals link.UserId
                        join role in builder.Table<Role>() on link.RoleId equals role.Id
                        select new RoleInfo { Login = user.Login, RoleName = role.Name};
            var command = roles.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [user].[Login] AS [Login], [role].[Name] AS [RoleName] " + 
                    "FROM [User] [user]  " + 
                        "JOIN [UserRoleLink] [link] ON [user].[Id] = [link].[UserId]  " + 
                        "JOIN [Role] [role] ON [link].[RoleId] = [role].[Id]"));
        }

        [Test]
        public void GetSqlCommandJoinWithSimplifiedSelectClauseIsTranslatedToSqlJoin()
        {
            var builder = new QueryBuilder();
            var roles = from user in builder.Table<User>()
                        join link in builder.Table<UserRoleLink>() on user.Id equals link.UserId
                        join role in builder.Table<Role>() on link.RoleId equals role.Id
                        select role;
            var command = roles.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [role].* " +
                    "FROM [User] [user]  " +
                        "JOIN [UserRoleLink] [link] ON [user].[Id] = [link].[UserId]  " +
                        "JOIN [Role] [role] ON [link].[RoleId] = [role].[Id]"));
        }

        [Test]
        public void GetSqlCommandLeftJoinIsTranslatedToSqlLeftJoin()
        {
            var builder = new QueryBuilder();
            var roles = builder.Table<User>()
                .LeftJoin(builder.Table<UserRoleLink>(), user => user.Id, link => link.UserId, (user, link) => new { user, link })
                .LeftJoin(builder.Table<Role>(), ul => ul.link.RoleId, role => role.Id, (ul, role) => new { ul, role })
                .Select(ulr => new { ulr.ul.user.Login, ulr.role.Name });

            var command = roles.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [user].[Login] AS [Login], [role].[Name] AS [Name] " +
                    "FROM [User] [user] " +
                        "LEFT JOIN [UserRoleLink] [link] ON [user].[Id] = [link].[UserId] " +
                        "LEFT JOIN [Role] [role] ON [link].[RoleId] = [role].[Id]"));
        }

        [Test]
        public void GetSqlCommandJoinToItselfIsTranslatedCorrectly()
        {
            var builder = new QueryBuilder();
            var parentAndChildren = from parent in builder.Table<Role>()
                                    join child in builder.Table<Role>() on parent.Id equals child.ParentId
                                    where child.Name != "Admin"
                                    select new { ParentName = parent.Name, ChildName = child.Name };
            var command = parentAndChildren.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [parent].[Name] AS [ParentName], [child].[Name] AS [ChildName] " +
                    "FROM [Role] [parent]  " + 
                    "JOIN [Role] [child] ON [parent].[Id] = [child].[ParentId] " +
                    "WHERE ([child].[Name]) <> (@p0)"));
        }

        [Test]
        public void GetSqlCommandJoinWithoutSelectClauseIsCalculatedCorrectly()
        {
            var builder = new QueryBuilder();
            var userRoles = from user in builder.Table<User>()
                            join link in builder.Table<UserRoleLink>() on user.Id equals link.UserId
                            select new { UserId = user.Id, link.RoleId };
            var command = userRoles.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [user].[Id] AS [UserId], [link].[RoleId] AS [RoleId] " +
                    "FROM [User] [user]  " +
                        "JOIN [UserRoleLink] [link] ON [user].[Id] = [link].[UserId]"));
        }

        [Test]
        public void GetSqlCommandJoinToItselfWithoutSelectClauseIsCalculatedCorrectly()
        {
            var builder = new QueryBuilder();
            var parentAndChildren = from parent in builder.Table<Role>()
                                    join child in builder.Table<Role>() on parent.Id equals child.ParentId
                                    select new { ParentName = parent.Name, ChildName = child.Name };
            var command = parentAndChildren.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [parent].[Name] AS [ParentName], [child].[Name] AS [ChildName] " +
                    "FROM [Role] [parent]  " +
                    "JOIN [Role] [child] ON [parent].[Id] = [child].[ParentId]"));
        }

        [Test]
        public void GetSqlCommandSimpleGroupByIsTranslatedToSqlGroupBy()
        {
            var builder = new QueryBuilder();
            var roleCounts = from l in builder.Table<UserRoleLink>()
                             group l by l.UserId into lg
                             select new { UserId = lg.Key, NumOfRoles = lg.Count(x => x.RoleId) };
            var command = roleCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [l].[UserId] AS [UserId], COUNT( ([l].[RoleId])) AS [NumOfRoles] " +
                    "FROM [UserRoleLink] [l] " +
                    "GROUP BY [l].[UserId]"));
        }

        [Test]
        public void GetSqlCommandGroupByWithCompoundKeyIsTranslatedToSqlGroupBy()
        {
            var builder = new QueryBuilder();
            var loginCounts = from u in builder.Table<User>()
                             group u by new { u.FirstName, u.LastName } into ug
                             select new { User = ug.Key.FirstName + " " + ug.Key.LastName, NumOfLogins = ug.Count(x => x.Login) };
            var command = loginCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT (([u].[FirstName]) + (@p0)) + ([u].[LastName]) AS [User], COUNT( ([u].[Login])) AS [NumOfLogins] " +
                    "FROM [User] [u] " +
                    "GROUP BY [u].[FirstName], [u].[LastName]"));
        }

        [Test]
        public void GetSqlCommandGroupByWithOnePropertyResultIsTranslatedToSqlGroupBy()
        {
            var builder = new QueryBuilder();
            var loginCounts = from u in builder.Table<User>()
                              group u.Login by new { u.FirstName, u.LastName } into ug
                              select new { User = ug.Key.FirstName + " " + ug.Key.LastName, NumOfLogins = ug.Count() };
            var command = loginCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT (([u].[FirstName]) + (@p0)) + ([u].[LastName]) AS [User], COUNT(*) AS [NumOfLogins] " +
                    "FROM [User] [u] " +
                    "GROUP BY [u].[FirstName], [u].[LastName]"));
        }

        [Test]
        public void GetSqlCommandGroupByWithCompoundResultIsTranslatedToSqlGroupBy()
        {
            var builder = new QueryBuilder();
            var loginCounts = from u in builder.Table<User>()
                              group new { u.FirstName, u.LastName } by u.Login into ug
                              select new { Login = ug.Key, NumOfNames = ug.Count(x => x.FirstName + " " + x.LastName) };
            var command = loginCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [u].[Login] AS [Login], COUNT( ((([u].[FirstName]) + (@p0)) + ([u].[LastName]))) AS [NumOfNames] " +
                    "FROM [User] [u] " +
                    "GROUP BY [u].[Login]"));
        }

        [Test]
        public void GetSqlCommandGroupByAfterJoinIsTranslatedToSqlGroupBy()
        {
            var builder = new QueryBuilder();
            var roleCounts = from u in builder.Table<User>()
                             join l in builder.Table<UserRoleLink>() on u.Id equals l.UserId
                             group l by u.Login into ug
                             select new { Login = ug.Key, NumOfRoles = ug.Count(x => x.RoleId) };
            var command = roleCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [u].[Login] AS [Login], COUNT( ([l].[RoleId])) AS [NumOfRoles] " +
                    "FROM [User] [u]  " + 
                        "JOIN [UserRoleLink] [l] ON [u].[Id] = [l].[UserId] " + 
                    "GROUP BY [u].[Login]"));
        }

        [Test]
        public void GetSqlCommandGroupByWithSumIsTranslatedToSqlGroupBy()
        {
            var builder = new QueryBuilder();
            var loginAttempts = from r in builder.Table<Role>()
                                join l in builder.Table<UserRoleLink>() on r.Id equals l.RoleId
                                join u in builder.Table<User>() on l.UserId equals u.Id
                                group u by r.Name into rg
                                select new { Role = rg.Key, LoginAttempts = rg.Sum(x => x.LoginAttempts) };
            var command = loginAttempts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [r].[Name] AS [Role], SUM( ([u].[LoginAttempts])) AS [LoginAttempts] " + 
                    "FROM [Role] [r]  " + 
                        "JOIN [UserRoleLink] [l] ON [r].[Id] = [l].[RoleId]  " + 
                        "JOIN [User] [u] ON [l].[UserId] = [u].[Id] " + 
                    "GROUP BY [r].[Name]"));
        }

        [Test]
        public void GetSqlCommandWhereBeforeGroupByIsTranslatedToWhere()
        {
            var builder = new QueryBuilder();
            var loginCounts = from u in builder.Table<User>()
                              where u.FirstName == "Jacek"
                              group u.Login by new { u.FirstName, u.LastName } into ug
                              select new { User = ug.Key.FirstName + " " + ug.Key.LastName, NumOfLogins = ug.Count() };
            var command = loginCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT (([u].[FirstName]) + (@p1)) + ([u].[LastName]) AS [User], COUNT(*) AS [NumOfLogins] " +
                    "FROM [User] [u] " +
                    "WHERE ([u].[FirstName]) = (@p0) " +
                    "GROUP BY [u].[FirstName], [u].[LastName]"));
        }

        [Test]
        public void GetSqlCommandWhereAfterGroupByIsTranslatedToHaving()
        {
            var builder = new QueryBuilder();
            var loginCounts = from u in builder.Table<User>()
                              group u.Login by new { u.FirstName, u.LastName } into ug
                              where ug.Key.FirstName == "Jacek"
                              select new { User = ug.Key.FirstName + " " + ug.Key.LastName, NumOfLogins = ug.Count() };
            var command = loginCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT (([u].[FirstName]) + (@p1)) + ([u].[LastName]) AS [User], COUNT(*) AS [NumOfLogins] " +
                    "FROM [User] [u] " +
                    "GROUP BY [u].[FirstName], [u].[LastName] " +
                    "HAVING ([u].[FirstName]) = (@p0)"));
        }

        [Test]
        public void GetSqlCommandJoinsWithSubqueriesAreAllowed()
        {
            var builder = new QueryBuilder();
            var result = from u in builder.Table<User>()
                         join l in builder.Table<UserRoleLink>().Where(l => l.RoleId != 1) on u.Id equals l.UserId
                         select u.Login;
            var command = result.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                "SELECT [u].[Login] " + 
                "FROM [User] [u]  " + 
                "JOIN (SELECT * FROM [UserRoleLink] [sq1_l] WHERE ([sq1_l].[RoleId]) <> (@sq1_p0)) [l] ON [u].[Id] = [l].[UserId]"));
        }

        [Test]
        public void GetSqlCommandSubqueriesAreAllowedInWhereClause()
        {
            var builder = new QueryBuilder();
            var subquery = from u in builder.Table<User>() 
                           where u.Login == "jacenty" 
                           select u.Id;
            var mainQuery = from u in builder.Table<User>() 
                            where subquery.Contains(u.Id) 
                            select u.FirstName + " " + u.LastName;
            var command = mainQuery.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT (([u].[FirstName]) + (@p1)) + ([u].[LastName]) " + 
                    "FROM [User] [u] " + 
                    "WHERE [u].[Id] IN (SELECT [sq1_u].[Id] FROM [User] [sq1_u] WHERE ([sq1_u].[Login]) = (@sq1_p0))"));
        }

        [Test]
        public void GetSqlCommandUsesNameResolverForTableAndColumnNames1()
        {
            var builder = new QueryBuilder(new PrefixBasedNameResolver());
            var roleCounts = from u in builder.Table<User>()
                             join l in builder.Table<UserRoleLink>() on u.Id equals l.UserId
                             group l by u.Login into ug
                             select new { Login = ug.Key, NumOfRoles = ug.Count(x => x.RoleId) };
            var command = roleCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [u].[col_Login] AS [Login], COUNT( ([l].[col_RoleId])) AS [NumOfRoles] " +
                    "FROM [TBL_User] [u]  " +
                        "JOIN [TBL_UserRoleLink] [l] ON [u].[col_Id] = [l].[col_UserId] " +
                    "GROUP BY [u].[col_Login]"));
        }

        [Test]
        public void GetSqlCommandUsesNameResolverForTableAndColumnNames2()
        {
            var builder = new SqlCommandBuilder(new PrefixBasedNameResolver());
            var source = new ModelQuerySource(typeof(User), new PrefixBasedNameResolver());
            var data = new SelectQueryData(builder, source);
            Expression<Func<User, object>> orderSpec = user => user.Login;
            Expression<Func<User, object>> selectSpec = user => new { user.FirstName, user.LastName };
            data.OrderByProperties.Add(Tuple.Create<LambdaExpression, bool>(orderSpec, true));
            data.SelectClause = selectSpec;
            var command = builder.GetSelectCommand(data);
            Assert.That(
                command.Command, 
                Is.EqualTo(
                    "SELECT [user].[col_FirstName] AS [FirstName], [user].[col_LastName] AS [LastName] " + 
                    "FROM [TBL_User] [user] " + 
                    "ORDER BY [user].[col_Login]"));
        }

        [Test]
        public void GetSqlCommandUnionIsTranslatedToSqlUnion()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>().Where(u => u.Login == "jacenty")
                .Union(builder.Table<User>().Where(u => u.Login == "kasia"))
                .ToSql();
            Assert.That(
                command.Command, 
                Is.EqualTo("(SELECT * FROM [User] [so0_u] WHERE ([so0_u].[Login]) = (@so0_p0) " + 
                            "UNION " + 
                            "SELECT * FROM [User] [so1_u] WHERE ([so1_u].[Login]) = (@so1_p0))"));
        }

        [Test]
        public void GetSqlCommandIntersectIsTranslatedToSqlIntersect()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>()
                .Intersect(builder.Table<User>().Where(u => u.Login == "kasia"))
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo("(SELECT * FROM [User] " +
                            "INTERSECT " +
                            "SELECT * FROM [User] [so1_u] WHERE ([so1_u].[Login]) = (@so1_p0))"));
        }

        [Test]
        public void GetSqlCommandExceptIsTranslatedToSqlExcept()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>()
                .Except(builder.Table<User>().Where(u => u.Login == "kasia"))
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo("(SELECT * FROM [User] " +
                            "EXCEPT " +
                            "SELECT * FROM [User] [so1_u] WHERE ([so1_u].[Login]) = (@so1_p0))"));
        }

        [Test]
        public void GetSqlCommandThreeSetOperationsAreParenthesizedCorrectly1()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>().Where(u => u.Login == "jacenty")
                .Union(builder.Table<User>().Where(u => u.Login == "kasia"))
                .Except(builder.Table<User>().Where(u => u.Login == "jacenty"))
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo("((SELECT * FROM [User] [so0_so0_u] WHERE ([so0_so0_u].[Login]) = (@so0_so0_p0) " +
                            "UNION " +
	                            "SELECT * FROM [User] [so0_so1_u] WHERE ([so0_so1_u].[Login]) = (@so0_so1_p0)) " +
                            "EXCEPT " +
	                            "SELECT * FROM [User] [so1_u] WHERE ([so1_u].[Login]) = (@so1_p0))"));
        }

        [Test]
        public void GetSqlCommandThreeSetOperationsAreParenthesizedCorrectly2()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>().Where(u => u.Login == "jacenty")
                .Union(builder.Table<User>().Where(u => u.Login == "kasia")
                    .Except(builder.Table<User>().Where(u => u.Login == "jacenty")))                
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "(SELECT * FROM [User] [so0_u] WHERE ([so0_u].[Login]) = (@so0_p0) " + 
                    "UNION " + 
                    "(SELECT * FROM [User] [so1_so0_u] WHERE ([so1_so0_u].[Login]) = (@so1_so0_p0) " + 
                        "EXCEPT SELECT * FROM [User] [so1_so1_u] WHERE ([so1_so1_u].[Login]) = (@so1_so1_p0)))"));
        }

        [Test]
        public void GetSqlCommandUnionCanBeFiltered()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>().Where(u => u.Login == "jacenty")
                .Union(builder.Table<User>().Where(u => u.Login == "kasia"))
                .Where(u => u.LoginAttempts == 0)
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo("SELECT * FROM " + 
                                "(SELECT * FROM [User] [sq0_so0_u] WHERE ([sq0_so0_u].[Login]) = (@sq0_so0_p0) " + 
                                "UNION " + 
                                "SELECT * FROM [User] [sq0_so1_u] WHERE ([sq0_so1_u].[Login]) = (@sq0_so1_p0)) [u] " + 
                            "WHERE ([u].[LoginAttempts]) = (@p2)"));
        }

        [Test]
        public void GetSqlCommandTranslatesWhereOnBoolPropertyToIntComparison()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>().Where(u => u.Disabled).ToSql();
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [u] WHERE [u].[Disabled] = 1"));
        }

        [Test]
        public void GetSqlCommandTranslatesWhereOnNegatedBoolPropertyToIntComparison()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>().Where(u => !u.Disabled).ToSql();
            Assert.That(command.Command, Is.EqualTo("SELECT * FROM [User] [u] WHERE NOT ([u].[Disabled] = 1)"));
        }

        [Test]
        public void GetSqlCommandTranslatesBoolPropertyInSelectClauseAsItself()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>().Select(u => u.Disabled).ToSql();
            Assert.That(command.Command, Is.EqualTo("SELECT [u].[Disabled] FROM [User] [u]"));
        }

        [Test]
        public void GetSqlCommandTranslatesAndWithBoolPropertyToIntComparison()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>().Where(u => u.Disabled && u.LoginAttempts == 0).ToSql();
            Assert.That(
                command.Command, 
                Is.EqualTo("SELECT * FROM [User] [u] WHERE ([u].[Disabled] = 1) AND (([u].[LoginAttempts]) = (@p0))"));
        }

        [Test]
        public void GetSqlCommandTranslatesMultipleWhereClausesToConditionsInOneClause()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>()
                .Where(u => u.FirstName == "Jacek")
                .Where(u => u.LastName == "Hełka").ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo("SELECT * FROM [User] [u] WHERE ([u].[FirstName]) = (@p0) AND ([u].[LastName]) = (@p1)"));
        }

        [Test]
        public void GetSqlCommandTranslatesWhereClauseAfterSelectToNestedSelect()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>()
                .Select(u => new { UserName = u.Login, Email = u.Email })
                .Where(u => u.UserName == "jacenty")
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT * FROM (SELECT [sq0_u].[Login] AS [UserName], [sq0_u].[Email] AS [Email] FROM [User] [sq0_u]) [u] " + 
                    "WHERE ([u].[UserName]) = (@p0)"));
        }

        [Test]
        public void GetSqlCommandTranslatesSelectClauseAfterSelectToNestedSelect()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>()
                .Select(u => new { UserName = u.Login, Email = u.Email })
                .Select(u => u.UserName)
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [u].[UserName] FROM (SELECT [sq0_u].[Login] AS [UserName], [sq0_u].[Email] AS [Email] FROM [User] [sq0_u]) [u]"));
        }

        [Test]
        public void GetSqlCommandTranslatesJoinClauseAfterSelectToNestedSelect()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>()
                .Select(u => new { UserId = u.Id, UserName = u.Login })
                .Join(builder.Table<UserRoleLink>(), u => u.UserId, l => l.UserId, (u, l) => new { u, l })
                .Join(builder.Table<Role>(), ul => ul.l.RoleId, r => r.Id, (ul, r) => new { ul.u.UserName, RoleName = r.Name })
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [u].[UserName] AS [UserName], [r].[Name] AS [RoleName] " + 
                    "FROM (SELECT [sq0_u].[Id] AS [UserId], [sq0_u].[Login] AS [UserName] FROM [User] [sq0_u]) [u]  " +
	                    "JOIN [UserRoleLink] [l] ON [u].[UserId] = [l].[UserId]  " +
	                    "JOIN [Role] [r] ON [l].[RoleId] = [r].[Id]"));
        }

        [Test]
        public void GetSqlCommandTranslatesLeftJoinClauseAfterSelectToNestedSelect()
        {
            var builder = new QueryBuilder();
            var command = builder.Table<User>()
                .Select(u => new { UserId = u.Id, UserName = u.Login })
                .LeftJoin(builder.Table<UserRoleLink>(), u => u.UserId, l => l.UserId, (u, l) => new { u, l })
                .LeftJoin(builder.Table<Role>(), ul => ul.l.RoleId, r => r.Id, (ul, r) => new { ul.u.UserName, RoleName = r.Name })
                .ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT [u].[UserName] AS [UserName], [r].[Name] AS [RoleName] " +
                    "FROM (SELECT [sq0_u].[Id] AS [UserId], [sq0_u].[Login] AS [UserName] FROM [User] [sq0_u]) [u] " +
                        "LEFT JOIN [UserRoleLink] [l] ON [u].[UserId] = [l].[UserId] " +
                        "LEFT JOIN [Role] [r] ON [l].[RoleId] = [r].[Id]"));
        }

        [Test]
        public void GetSqlCommandMultipleWhereClausesAfterGroupByIsTranslatedToHavingWithMultipleConditions()
        {
            var builder = new QueryBuilder();
            var loginCounts = from u in builder.Table<User>()
                              group u.Login by new { u.FirstName, u.LastName } into ug
                              where ug.Key.FirstName == "Jacek"
                              where ug.Key.LastName == "Hełka"
                              select new { User = ug.Key.FirstName + " " + ug.Key.LastName, NumOfLogins = ug.Count() };
            var command = loginCounts.ToSql();
            Assert.That(
                command.Command,
                Is.EqualTo(
                    "SELECT (([u].[FirstName]) + (@p2)) + ([u].[LastName]) AS [User], COUNT(*) AS [NumOfLogins] " +
                    "FROM [User] [u] " +
                    "GROUP BY [u].[FirstName], [u].[LastName] " +
                    "HAVING ([u].[FirstName]) = (@p0) AND ([u].[LastName]) = (@p1)"));
        }

    }
}
