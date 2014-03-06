using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tests1 = new SqlCommandBuilderTests();
            tests1.GetSqlCommandTranslatesWhereOnBoolPropertyToIntComparison();
            tests1.GetSqlCommandTranslatesWhereOnNegatedBoolPropertyToIntComparison();
            tests1.GetSqlCommandTranslatesBoolPropertyInSelectClauseAsItself();
            tests1.GetSqlCommandTranslatesAndWithBoolPropertyToIntComparison();
            //tests1.GetSqlCommandUnionIsTranslatedToSqlUnion();
            //tests1.GetSqlCommandExceptIsTranslatedToSqlExcept();
            //tests1.GetSqlCommandIntersectIsTranslatedToSqlIntersect();
            //tests1.GetSqlCommandThreeSetOperationsAreParenthesizedCorrectly2();
            //tests1.GetSqlCommandUnionCanBeFiltered();
            //tests1.GetSqlCommandUsesNameResolverForTableAndColumnNames1();
            //tests1.GetSqlCommandUsesNameResolverForTableAndColumnNames2();
            //tests1.GetSqlCommandWhereBeforeGroupByIsTranslatedToWhere();
            //tests1.GetSqlCommandGroupByWithSumIsTranslatedToSqlGroupBy();
            //tests1.GetSqlCommandWhereAfterGroupByIsTranslatedToHaving();
            //tests1.GetSqlCommandJoinsWithSubqueriesAreAllowed();
            //tests1.GetSqlCommandSubqueriesAreAllowedInWhereClause();
            //tests1.GetSqlCommandSimpleGroupByIsTranslatedToSqlGroupBy();
            //tests1.GetSqlCommandGroupByWithCompoundKeyIsTranslatedToSqlGroupBy();
            //tests1.GetSqlCommandGroupByWithOnePropertyResultIsTranslatedToSqlGroupBy();
            //tests1.GetSqlCommandGroupByWithCompoundResultIsTranslatedToSqlGroupBy();
            //tests1.GetSqlCommandGroupByAfterJoinIsTranslatedToSqlGroupBy();
            /*tests1.GetSqlCommandTranslatesSubtractDaysToDateDiffDays();
            tests1.GetSqlCommandJoinIsTranslatedToSqlJoin();
            tests1.GetSqlCommandJoinWithoutSelectClauseIsCalculatedCorrectly();
            tests1.GetSqlCommandLeftJoinIsTranslatedToSqlLeftJoin();
            tests1.GetSqlCommandJoinToItselfIsTranslatedCorrectly();
            tests1.GetSqlCommandJoinToItselfWithoutSelectClauseIsCalculatedCorrectly();
            tests1.GetSqlCommandJoinWithSimplifiedSelectClauseIsTranslatedToSqlJoin();*/
            //tests1.ToSqlVsToStringPerformance();
            //tests1.GetSqlCommandContainsMethodIsTranslatedToLikeOperator();
            //tests1.GetSqlCommandGeneratesSimplestSelectIfNoAdditionalArgumentsPassed();
            //tests1.GetSqlCommandGroupByAfterJoinIsTranslatedToSqlGroupBy();
            /*var tests3 = new QueryIntegrationTests();
            tests3.SetUp();
            tests3.SimpleSelectExecutesCorrectly();
            tests3.SelectWithWhereExecutesCorrectly();
            tests3.StringFunctionsAreCalculatedCorrectly();
            tests3.DateTimeFunctionsAreCalculatedCorrectly();
            tests3.MathFunctionsAreCalculatedCorrectly();
            tests3.TypeCastsAreCalculatedCorrectly();
            tests3.SkipExecutesCorrectly();
            tests3.TakeExecutesCorrectly();
            tests3.SkipAndTakeExecutesCorrectly();
            tests3.WhereOnBoolPropertyExecutesCorrectly();
            var tests4 = new DapperIntegrationTests();
            tests4.SetUp();
            tests4.QueryOverModelExecutesCorreclty();
            tests4.QueryOverModelExecutesFastEnough();*/
            Console.WriteLine("Press [ENTER]");
            Console.ReadLine();
        }
    }
}
