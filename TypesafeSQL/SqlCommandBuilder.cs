using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The class responsible for an SQL command generation.
    /// </summary>
    public class SqlCommandBuilder
    {
        private static Dictionary<ExpressionType, string> operatorTranslations = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, "=" },
            { ExpressionType.NotEqual, "<>" },
            { ExpressionType.LessThan, "<" },
            { ExpressionType.LessThanOrEqual, "<=" },
            { ExpressionType.GreaterThan, ">" },
            { ExpressionType.GreaterThanOrEqual, ">=" },
            { ExpressionType.AndAlso, "AND" },
            { ExpressionType.And, "AND" },
            { ExpressionType.OrElse, "OR" },
            { ExpressionType.Or, "OR" },
            { ExpressionType.Not, "NOT" },
            { ExpressionType.Add, "+" },
            { ExpressionType.Subtract, "-" },
            { ExpressionType.Multiply, "*" },
            { ExpressionType.Divide, "/" },
            { ExpressionType.Negate, "-" },
            { ExpressionType.Quote, "" },
        };

        private static Dictionary<MethodInfo, string> methodTranslations = new Dictionary<MethodInfo, string>
        {
            // String 
            { typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), "({0}) LIKE (({1}) + '%')" },
            { typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), "({0}) LIKE ('%' + ({1}) + '%')" },
            { typeof(string).GetMethod("Substring", new Type[] { typeof(int), typeof(int) }), "SUBSTRING({0}, {1} + 1, {2})" },
            { typeof(string).GetMethod("Replace", new Type[] { typeof(string), typeof(string) }), "REPLACE({0}, {1}, {2})" },
            { typeof(string).GetMethod("TrimStart", new Type[] { typeof(char[]) }), "LTRIM({0})" },
            { typeof(string).GetMethod("TrimEnd", new Type[] { typeof(char[]) }), "RTRIM({0})" },
            { typeof(string).GetMethod("Trim", Type.EmptyTypes), "LTRIM(RTRIM({0}))" },
            { typeof(string).GetMethod("ToUpper", Type.EmptyTypes), "UPPER({0})" },
            { typeof(string).GetMethod("ToLower", Type.EmptyTypes), "LOWER({0})" },
            { typeof(string).GetMethod("IndexOf", new Type[] { typeof(string) }), "CHARINDEX({1}, {0}) - 1" },
            { typeof(int).GetMethod("ToString", Type.EmptyTypes), "CONVERT(VARCHAR(MAX), {0})" },
            { typeof(long).GetMethod("ToString", Type.EmptyTypes), "CONVERT(VARCHAR(MAX), {0})" },
            { typeof(double).GetMethod("ToString", Type.EmptyTypes), "CONVERT(VARCHAR(MAX), {0})" },
            { typeof(decimal).GetMethod("ToString", Type.EmptyTypes), "CONVERT(VARCHAR(MAX), {0})" },
            { typeof(int).GetMethod("Parse", new Type[] { typeof(string) }), "CONVERT(INT, {1})" },
            { typeof(long).GetMethod("Parse", new Type[] { typeof(string) }), "CONVERT(BIGINT, {1})" },
            { typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), "CONVERT(FLOAT, {1})" },
            { typeof(decimal).GetMethod("Parse", new Type[] { typeof(string) }), "CONVERT(DECIMAL, {1})" },

            // DateTime 
            { typeof(DateTime).GetMethod("Parse", new Type[] { typeof(string) }), "CONVERT(DATETIME, {1})" },
            { typeof(DateTime).GetMethod("Subtract", new Type[] { typeof(DateTime) }), "DATEDIFF({0}, {2}, {1})" },
            { typeof(DateTime).GetMethod("AddDays", new Type[] { typeof(int) }), "DATEADD(day, {1}, {0})" },
            { typeof(DateTime).GetMethod("AddMonths", new Type[] { typeof(int) }), "DATEADD(month, {1}, {0})" },
            { typeof(DateTime).GetMethod("AddYears", new Type[] { typeof(int) }), "DATEADD(year, {1}, {0})" },
            { typeof(DateTime).GetMethod("AddHours", new Type[] { typeof(int) }), "DATEADD(hour, {1}, {0})" },
            { typeof(DateTime).GetMethod("AddMinutes", new Type[] { typeof(int) }), "DATEADD(minute, {1}, {0})" },
            { typeof(DateTime).GetMethod("AddSeconds", new Type[] { typeof(int) }), "DATEADD(second, {1}, {0})" },
            { typeof(DateTime).GetMethod("AddMilliseconds", new Type[] { typeof(int) }), "DATEADD(millisecond, {1}, {0})" },

            // Numbers/Math
            { typeof(Math).GetMethod("Abs", new Type[] { typeof(int) }), "ABS({1})" },
            { typeof(Math).GetMethod("Abs", new Type[] { typeof(long) }), "ABS({1})" },
            { typeof(Math).GetMethod("Abs", new Type[] { typeof(float) }), "ABS({1})" },
            { typeof(Math).GetMethod("Abs", new Type[] { typeof(double) }), "ABS({1})" },
            { typeof(Math).GetMethod("Abs", new Type[] { typeof(decimal) }), "ABS({1})" },
            { typeof(Math).GetMethod("Acos", new Type[] { typeof(double) }), "ACOS({1})" },
            { typeof(Math).GetMethod("Asin", new Type[] { typeof(double) }), "ASIN({1})" },
            { typeof(Math).GetMethod("Atan", new Type[] { typeof(double) }), "ATAN({1})" },
            { typeof(Math).GetMethod("Atan2", new Type[] { typeof(double), typeof(double) }), "ATN2({1}, {2})" },
            { typeof(Math).GetMethod("Ceiling", new Type[] { typeof(decimal) }), "CEILING({1})" },
            { typeof(Math).GetMethod("Ceiling", new Type[] { typeof(double) }), "CEILING({1})" },
            { typeof(Math).GetMethod("Cos", new Type[] { typeof(double) }), "COS({1})" },
            //{ typeof(Math).GetMethod("Acos", new Type[] { typeof(double) }), "ACOS({1})" },
            { typeof(Math).GetMethod("Exp", new Type[] { typeof(double) }), "EXP({1})" },
            { typeof(Math).GetMethod("Floor", new Type[] { typeof(decimal) }), "FLOOR({1})" },
            { typeof(Math).GetMethod("Floor", new Type[] { typeof(double) }), "FLOOR({1})" },
            { typeof(Math).GetMethod("Log", new Type[] { typeof(double) }), "LOG({1})" },
            { typeof(Math).GetMethod("Log10", new Type[] { typeof(double) }), "LOG10({1})" },
            { typeof(Math).GetMethod("Pow", new Type[] { typeof(double), typeof(double) }), "POWER({1}, {2})" },
            { typeof(Math).GetMethod("Round", new Type[] { typeof(decimal), typeof(int) }), "ROUND({1}, {2})" },
            { typeof(Math).GetMethod("Sign", new Type[] { typeof(int) }), "SIGN({1})" },
            { typeof(Math).GetMethod("Sign", new Type[] { typeof(long) }), "SIGN({1})" },
            { typeof(Math).GetMethod("Sign", new Type[] { typeof(float) }), "SIGN({1})" },
            { typeof(Math).GetMethod("Sign", new Type[] { typeof(double) }), "SIGN({1})" },
            { typeof(Math).GetMethod("Sign", new Type[] { typeof(decimal) }), "SIGN({1})" },
            { typeof(Math).GetMethod("Sqrt", new Type[] { typeof(double) }), "SQRT({1})" },
            { typeof(Math).GetMethod("Tan", new Type[] { typeof(double) }), "TAN({1})" },
            { typeof(Random).GetMethod("NextDouble", Type.EmptyTypes), "RAND()" },
            { typeof(Random).GetMethod("Next", new Type[] { typeof(int) }), "FLOOR(RAND()*{1})" },

            // Aggregation/Subqueries
            { typeof(IGroup<,>).GetMethod("Count", new Type[] { }), "COUNT(*)" },
            { typeof(IGroup<,>).GetMethods().Single(m => m.Name == "Count" && m.GetParameters().Length == 1), "COUNT({1})" },
            { typeof(IGroup<,>).GetMethod("Sum"), "SUM({1})" },
            { typeof(IGroup<,>).GetMethod("Average"), "AVG({1})" },
            { typeof(IGroup<,>).GetMethod("Min"), "MIN({1})" },
            { typeof(IGroup<,>).GetMethod("Max"), "MAX({1})" },
            { typeof(IQuery<>).GetMethod("Contains"), "{1} IN ({0})" },
        };

        private Dictionary<Type, string> typeTranslations = new Dictionary<Type, string>
        {
            { typeof(int), "INT" },
            { typeof(long), "BIGINT" },
            { typeof(float), "REAL" },
            { typeof(double), "FLOAT" },
            { typeof(string), "VARCHAR(MAX)" },
            { typeof(DateTime), "DATETIME" },
            { typeof(decimal), "DECIMAL(10, 8)" },
        };

        private static Dictionary<MemberInfo, string> propertyTranslations = new Dictionary<MemberInfo, string>
        {
            // String
            { typeof(string).GetProperty("Length"), "LEN({0})" },

            // DateTime
            { typeof(DateTime).GetProperty("Year"), "YEAR({0})" },
            { typeof(DateTime).GetProperty("Month"), "MONTH({0})" },
            { typeof(DateTime).GetProperty("Day"), "DAY({0})" },
            { typeof(DateTime).GetProperty("DayOfWeek"), "DATEPART(weekday, {0}) - 1" },
            { typeof(DateTime).GetProperty("DayOfYear"), "DATEPART(dayofyear, {0})" },
            { typeof(DateTime).GetProperty("Hour"), "DATEPART(hour, {0})" },
            { typeof(DateTime).GetProperty("Minute"), "DATEPART(minute, {0})" },
            { typeof(DateTime).GetProperty("Second"), "DATEPART(second, {0})" },
            { typeof(DateTime).GetProperty("Millisecond"), "DATEPART(millisecond, {0})" },
            { typeof(DateTime).GetProperty("Now"), "GETDATE()" },
            { typeof(DateTime).GetProperty("UtcNow"), "GETUTCDATE()" },
            { typeof(DateTime).GetProperty("Today"), "GETDATE()" },
            { typeof(TimeSpan).GetProperty("Days"), "day" },
            { typeof(TimeSpan).GetProperty("TotalDays"), "day" },
            { typeof(TimeSpan).GetProperty("TotalHours"), "hour" },
            { typeof(TimeSpan).GetProperty("TotalMinutes"), "minute" },
            { typeof(TimeSpan).GetProperty("TotalSeconds"), "second" },
            { typeof(TimeSpan).GetProperty("TotalMilliseconds"), "millisecond" },
        };

        private INameResolver nameResolver;
        private string subQueryPrefix;
        private int lastSubQueryId;
        private string lastTableAlias;
        private IDictionary<string, object> parameters;
        private IDictionary<MemberInfo, string> aliases;
        private string groupKeySql;

        /// <summary>
        /// Initializes a new instance of the SQLCommandBuilder class.
        /// </summary>
        /// <param name="nameResolver">
        /// The table/column name resolver.
        /// </param>
        public SqlCommandBuilder(INameResolver nameResolver)
        {
            Check.NotNull(nameResolver, "nameResolver");
            this.nameResolver = nameResolver;
        }

        /// <summary>
        /// Generates an SQL command.
        /// </summary>
        /// <param name="data">
        /// The data collected by a query instance, needed for the command generation.
        /// </param>
        /// <param name="subQueryPrefix">
        /// The prefix used for parameter and alias name construction in subqueries.
        /// </param>
        /// <returns>
        /// The SQL command, generated from QueryData instance.
        /// </returns>
        public virtual ParameterizedSql GetSelectCommand(SelectQueryData data, string subQueryPrefix = "")
        {
            Check.NotNull(data, "data");
            Check.NotNull(data.ModelType, "data.ModelType");
            this.subQueryPrefix = subQueryPrefix;
            lastSubQueryId = 0;
            lastTableAlias = null;
            //parameters = new Dictionary<string, object>();
            parameters = new System.Dynamic.ExpandoObject();
            aliases = new Dictionary<MemberInfo, string>();
            var aliasedTableName = GetAliasedTableName(data);
            var joinClauses = GetAllJoinsText(data);
            var whereClause = GetWhereClauseText(data);
            var groupClause = GetGroupClauseText(data);
            var havingClause = GetHavingClauseText(data);
            var orderByClause = GetOrderByClauseText(data);
            var distinctClause = data.Distinct ? "DISTINCT " : "";
            var selectClauseExpr = data.SelectClause ?? data.Joins.Select(j => j.ResultSelector).LastOrDefault();
            var selectClause = GetSelectClauseText(data);
            var offsetFetchClause = GetOffsetFetchClauseText(data);
            string command = string.Format("SELECT {0}{1} FROM {2}{3}{4}{5}{6}{7}{8}",
                distinctClause, 
                selectClause, 
                aliasedTableName, 
                joinClauses, 
                whereClause, 
                groupClause, 
                havingClause, 
                orderByClause, 
                offsetFetchClause);
            return new ParameterizedSql
            {
                Command = command,
                Parameters = parameters
            };
        }

        private object GetGroupClauseText(SelectQueryData data)
        {
            if (data.GroupByKey == null)
                return "";
            var newExpr = data.GroupByKey.Body as NewExpression;
            if (newExpr != null)
                return string.Format(" GROUP BY {0}", string.Join(", ", newExpr.Arguments.Select(expr => ExpressionToSql(expr, false))));
            else
            {
                groupKeySql = ExpressionToSql(data.GroupByKey.Body, false);
                return string.Format(" GROUP BY {0}", groupKeySql);
            }
        }

        private string GetOffsetFetchClauseText(SelectQueryData data)
        {
            var offsetFetchClause = (data.SkipRows != 0 || data.TakeRows != 0) ? string.Format(" OFFSET {0} ROWS", data.SkipRows) : "";
            if (data.TakeRows != 0)
                offsetFetchClause += string.Format(" FETCH NEXT {0} ROWS ONLY", data.TakeRows);
            return offsetFetchClause;
        }

        private string GetAliasedTableName(SelectQueryData data)
        {
            string alias = DetermineMainTableAlias(data);
            var subQuerySql = data.FromData.GetSqlCommandOrTableName("sq" + (lastSubQueryId++) + "_");
            foreach (var param in subQuerySql.Parameters)
                parameters[param.Key] = param.Value;
            return subQuerySql.Command + (alias != null ? " [" + alias + "]" : "");
        }

        private string GetAllJoinsText(SelectQueryData data)
        {
            return string.Join("", data.Joins.Select(GetJoinClauseText));
        }

        private string GetJoinSourceSql(IQuerySource innerData)
        {
            var subQuerySql = innerData.GetSqlCommandOrTableName(string.Format("sq{0}_", lastSubQueryId++));
            foreach (var param in subQuerySql.Parameters)
                parameters[param.Key] = param.Value;
            return subQuerySql.Command;
        }

        private string GetJoinClauseText(SelectQueryData.JoinSpec join)
        {            
            var alias = subQueryPrefix + join.ResultSelector.Parameters[1].Name;
            var member = FindMemberSetByParameter(join, 1);
            if (member != null)
                aliases[member] = alias;
            string innerData = "[" + nameResolver.ResolveTableName(join.InnerData.ModelType) + "]";
            var clause = string.Format(" {0} JOIN {1} [{2}] ON {3} = {4}", 
                join.JoinType,
                GetJoinSourceSql(join.InnerData),
                alias,
                GetOuterKeyColumn(join.OuterKeySelector.Body),
                GetInnerKeyColumn(join.InnerKeySelector.Body, alias));
            if (lastTableAlias != null)
                lastTableAlias = alias;
            return clause;
        }

        private MemberInfo FindMemberSetByParameter(SelectQueryData.JoinSpec join, int paramIndex)
        {
            var newExpr = join.ResultSelector.Body as NewExpression;
            if (newExpr != null)
            {
                var index = newExpr.Arguments.TakeWhile(a => a != join.ResultSelector.Parameters[paramIndex]).Count();
                if (index < newExpr.Members.Count)
                    return newExpr.Members[index];
            }
            return null;
        }

        private string GetInnerKeyColumn(Expression selector, string tableAlias)
        {
            return string.Format("[{0}].[{1}]", tableAlias, nameResolver.ResolveColumnName((selector as MemberExpression).Member));
        }

        private string GetOuterKeyColumn(Expression selector)
        {
            return MemberExprToSql(selector as MemberExpression, false);
        }

        private string GetSelectClauseText(SelectQueryData data)
        {
            var selectClause = data.SelectClause ?? data.Joins.Select(j => j.ResultSelector).LastOrDefault();
            if (selectClause == null)
                return "*";
            var memberInitExpr = selectClause.Body as MemberInitExpression;
            if (memberInitExpr != null)
                return string.Join(", ", memberInitExpr.Bindings.Select(MemberBindingToSql));
            var newExpr = selectClause.Body as NewExpression;
            if (newExpr != null)
                return ConstructorCallToSql(newExpr.Members, newExpr.Arguments);
            return ExpressionToSql(selectClause.Body, false) + (selectClause.Body is ParameterExpression ? ".*" : "");
        }

        private string ConstructorCallToSql(ICollection<MemberInfo> members, ICollection<Expression> arguments)
        {
            var columns = members.Zip(
                arguments, 
                (member, arg) => string.Format("{0} AS [{1}]", ExpressionToSql(arg, false), member.Name));
            return string.Join(", ", columns);
        }

        private string MemberBindingToSql(MemberBinding binding)
        {
            var assignBinding = binding as MemberAssignment;
            if (assignBinding != null)
                return string.Format("{0} AS [{1}]", ExpressionToSql(assignBinding.Expression, false), binding.Member.Name);
            return "[" + binding.Member.Name + "]";
        }

        private string DetermineMainTableAlias(SelectQueryData data)
        {
            var join = data.Joins.FirstOrDefault();
            if (join != null)
            {
                var alias = subQueryPrefix + join.ResultSelector.Parameters[0].Name;
                var member = FindMemberSetByParameter(join, 0);
                if (member != null)
                {
                    aliases[member] = alias;
                    lastTableAlias = alias;
                }
                return alias;
            }
            else
            {
                var allParams = new[] { data.SelectClause, data.GroupByKey, data.GroupByElement, data.WhereClause }
                    .Where(c => c != null)
                    .Union(data.OrderByProperties.Select(o => o.Item1))
                    .SelectMany(c => c.Parameters)
                    .GroupBy(p => p.Type)
                    .ToDictionary(g => g.Key, g => subQueryPrefix + g.First().Name);
                return DetermineAlias(data.ModelType, allParams, data.Joins.Count() + 1);
            }
        }

        private string DetermineAlias(Type modelType, IDictionary<Type, string> clauseParams, int depth)
        {
            if (depth == 0)
                return null;
            string alias;
            if (clauseParams.TryGetValue(modelType, out alias))
            {
                lastTableAlias = alias;
                return alias;
            }
            else
            {
                var paramProperties = clauseParams
                    .Keys.SelectMany(t => t.GetProperties()).ToDictionary(p => p.PropertyType, p => subQueryPrefix + p.Name);
                return DetermineAlias(modelType, paramProperties, depth - 1);
            }
        }

        private object GetHavingClauseText(SelectQueryData data)
        {
            return data.HavingClause != null
                ? " HAVING " + ExpressionToSql(data.HavingClause.Body, true)
                : "";
        }


        protected virtual string GetWhereClauseText(SelectQueryData data)
        {
            return data.WhereClause != null
                ? " WHERE " + ExpressionToSql(data.WhereClause.Body, true)
                : "";
        }

        private string ExpressionToSql(Expression expression, bool isCondition)
        {
            var unaryExpr = expression as UnaryExpression;
            if (unaryExpr != null)
                return UnaryExprToSql(unaryExpr);

            var binaryExpr = expression as BinaryExpression;
            if (binaryExpr != null)
                return BinaryExprToSql(binaryExpr);

            var conditionalExpr = expression as ConditionalExpression;
            if (conditionalExpr != null)
                return ConditionalExprToSql(conditionalExpr);

            var memberExpr = expression as MemberExpression;
            if (memberExpr != null)
                return MemberExprToSql(memberExpr, isCondition);

            var callExpr = expression as MethodCallExpression;
            if (callExpr != null)
                return CallExprToSql(callExpr);

            var newExpr = expression as NewExpression;
            if (newExpr != null)
                return NewExprToSql(newExpr);

            var newArrayExpr = expression as NewArrayExpression;
            if (newArrayExpr != null)
                return null;

            var constExpr = expression as ConstantExpression;
            if (constExpr != null)
                return ConstExprToSql(constExpr);

            var paramExpr = expression as ParameterExpression;
            if (paramExpr != null)
                return ParamExprToSql(paramExpr);

            var lambdaExpr = expression as LambdaExpression;
            if (lambdaExpr != null)
                return LambdaExprToSql(lambdaExpr);

            throw new NotSupportedException(string.Format("Can not translate expression: {0}.", expression.ToString()));
        }

        private string LambdaExprToSql(LambdaExpression lambdaExpr)
        {
            return ExpressionToSql(lambdaExpr.Body, false);
        }

        private string ParamExprToSql(ParameterExpression paramExpr)
        {
            string alias = lastTableAlias ?? paramExpr.Name;
            return "[" + alias + "]";
        }

        private string ConstExprToSql(ConstantExpression constExpr)
        {
            if (constExpr.Value == null)
                return "NULL";
            return AddParameter(constExpr.Value);
        }

        private string NewExprToSql(NewExpression newExpr)
        {
            var args = newExpr.Arguments.Select(argExpr => ExpressionToSql(argExpr, false)).ToArray();
            return NewExprToSql(newExpr.Constructor, args);
        }

        private string CallExprToSql(MethodCallExpression callExpr)
        {
            var callee = callExpr.Object != null ? ExpressionToSql(callExpr.Object, false) : null;
            var args = callExpr.Arguments.Select(argExpr => ExpressionToSql(argExpr, false));
            return MethodCallToSql(callExpr.Method, new[] { callee }.Concat(args).ToArray());
        }

        private string MemberExprToSql(MemberExpression memberExpr, bool isCondition)
        {
            if (groupKeySql != null &&
                memberExpr.Member.DeclaringType.IsGenericType &&
                memberExpr.Member.Name == "Key" &&
                memberExpr.Member.DeclaringType.GetGenericTypeDefinition() == typeof(IGroup<,>))
                return groupKeySql;
            if (memberExpr.Member.DeclaringType == typeof(TimeSpan))
                return TimeStampPropToSql(memberExpr);
            var constExpr = memberExpr.Expression as ConstantExpression;
            if (constExpr != null)
                return ClosureMemberToSql(constExpr.Value, memberExpr.Member);
            var target = memberExpr.Expression != null ? ExpressionToSql(memberExpr.Expression, false) : null;
            var standard = MemberAccessToSql(memberExpr.Member, target);
            if (standard != null)
                return standard;
            var paramExpr = memberExpr.Expression as ParameterExpression;
            if (paramExpr != null)
            {
                var suffix = isCondition && memberExpr.Type == typeof(bool) ? " = 1" : "";
                return string.Format("{0}.[{1}]{2}", 
                    ParamExprToSql(paramExpr), 
                    nameResolver.ResolveColumnName(memberExpr.Member),
                    suffix);
            }
            var innerMemberExpr = memberExpr.Expression as MemberExpression;
            if (innerMemberExpr != null)
                return InnerMemberExprToSql(innerMemberExpr, memberExpr.Member);
            throw new NotSupportedException(string.Format("Can not translate expression: {0}.", memberExpr.ToString()));
        }

        private string ClosureMemberToSql(object closure, MemberInfo member)
        {
            object value = GetMemberValue(closure, member);
            var subQuery = value as IQuery;
            if (subQuery != null)
            {
                var subQuerySql = subQuery.Data.GetSqlCommand(string.Format("sq{0}_", lastSubQueryId++));
                foreach (var param in subQuerySql.Parameters)
                    parameters[param.Key] = param.Value;
                return subQuerySql.Command;
            }
            return AddParameter(value);
        }

        private string InnerMemberExprToSql(MemberExpression innerMemberExpr, MemberInfo outerMember)
        {
            string alias;
            if (!aliases.TryGetValue(innerMemberExpr.Member, out alias))
                alias = lastTableAlias;
            return string.Format("[{0}].[{1}]", alias, nameResolver.ResolveColumnName(outerMember));
        }

        private object GetMemberValue(object obj, MemberInfo member)
        {
            var property = member as PropertyInfo;
            if (property != null)
                return property.GetValue(obj);
            var field = member as FieldInfo;
            if (field != null)
                return field.GetValue(obj);
            throw new NotSupportedException(string.Format("Can not access closure member {0}.", member));
        }

        private string TimeStampPropToSql(MemberExpression memberExpr)
        {
            var member = MemberAccessToSql(memberExpr.Member, null);
            var callExpr = memberExpr.Expression as MethodCallExpression;
            if (callExpr != null)
            {
                string callPattern;
                var callee = ExpressionToSql(callExpr.Object, false);
                var args = callExpr.Arguments.Select(argExpr => ExpressionToSql(argExpr, false));
                if (!methodTranslations.TryGetValue(callExpr.Method, out callPattern))
                    throw new NotSupportedException(string.Format("Can not translate method {0}", callExpr.Method));
                var expr = string.Format(callPattern, new[] { member, callee }.Concat(args).ToArray());
                return memberExpr.Type == typeof(double) ? string.Format("CONVERT(FLOAT, {0})", expr) : expr;
            }

            throw new NotSupportedException(string.Format("Can not translate expression {0}", memberExpr));
        }

        private string ConditionalExprToSql(ConditionalExpression conditionalExpr)
        {
            var test = ExpressionToSql(conditionalExpr.Test, true);
            var ifTrue = ExpressionToSql(conditionalExpr.IfTrue, false);
            var ifFalse = ExpressionToSql(conditionalExpr.IfFalse, false);
            return string.Format("IIF({0}, {1}, {2})", test, ifTrue, ifFalse);
        }

        private string BinaryExprToSql(BinaryExpression binaryExpr)
        {
            var isBool = binaryExpr.Type == typeof(Boolean);
            string left = ExpressionToSql(binaryExpr.Left, isBool);
            string right = ExpressionToSql(binaryExpr.Right, isBool);
            string op = OperatorToSql(binaryExpr.NodeType, left == "NULL" || right == "NULL");
            return string.Format("({0}) {1} ({2})", left, op, right);
        }

        private string UnaryExprToSql(UnaryExpression unaryExpr)
        {
            string operand = ExpressionToSql(unaryExpr.Operand, unaryExpr.Type == typeof(Boolean));
            if (unaryExpr.NodeType == ExpressionType.Convert || unaryExpr.NodeType == ExpressionType.ConvertChecked)
                return string.Format("CONVERT({0}, {1})", typeTranslations[unaryExpr.Type], operand);
            return string.Format("{0} ({1})", OperatorToSql(unaryExpr.NodeType, operand == "NULL"), operand);
        }

        private string NewExprToSql(ConstructorInfo ctor, string[] args)
        {
            var expandedArgs = args.Concat(Enumerable.Repeat("0", 7 - args.Count()));
            if (ctor.DeclaringType == typeof(DateTime))
                return string.Format("DATETIMEFROMPARTS({0})", string.Join(", ", expandedArgs));
            if (ctor.DeclaringType == typeof(Random))
                return "";

            throw new NotSupportedException(string.Format("Can not translate object creation: {0}", ctor));
        }

        private string AddParameter(object value)
        {
            string lastParam = string.Format("@{0}p{1}", subQueryPrefix, parameters.Count);
            parameters[lastParam] = value;
            return lastParam;
        }

        private string OperatorToSql(ExpressionType exprType, bool anyOperandNull)
        {
            if (anyOperandNull && exprType == ExpressionType.Equal)
                return "IS";
            if (anyOperandNull && exprType == ExpressionType.NotEqual)
                return "IS NOT";
            string opSql;
            if (!operatorTranslations.TryGetValue(exprType, out opSql))
                throw new NotSupportedException(string.Format("Operator {0} is not supported.", exprType.ToString()));
            return opSql;
        }

        private string MethodCallToSql(MethodInfo method, string[] argumentExpressions)
        {
            string callPattern;
            if (methodTranslations.TryGetValue(method, out callPattern))
                return string.Format(callPattern, argumentExpressions);
            if (method.DeclaringType.IsGenericType)
            {
                method = method.DeclaringType.GetGenericTypeDefinition().GetMethods()
                    .First(m => m.Name == method.Name && m.GetParameters().Length == method.GetParameters().Length);
                if (methodTranslations.TryGetValue(method, out callPattern))
                    return string.Format(callPattern, argumentExpressions);
            }
            throw new NotSupportedException(string.Format("Can not translate method {0}", method));
        }

        private string MemberAccessToSql(MemberInfo member, string callee)
        {
            string callPattern;
            if (!propertyTranslations.TryGetValue(member, out callPattern))
                return null;
            return string.Format(callPattern, callee);
        }

        protected virtual string GetOrderByClauseText(SelectQueryData data)
        {
            return data.OrderByProperties.Any()
                ? " ORDER BY " + string.Join(", ", data.OrderByProperties
                    .Select(prop => ExpressionToSql(prop.Item1.Body, false) + (prop.Item2 ? "" : " DESC"))) 
                : "";
        }
    }
}
