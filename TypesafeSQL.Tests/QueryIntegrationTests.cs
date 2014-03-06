using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypesafeSQL;

namespace TypesafeSQL.Tests
{
    [TestFixture]
    class QueryIntegrationTests
    {
        private QueryBuilder builder;
        private QueryExecutor executor;

        [SetUp]
        public void SetUp()
        {
            builder = new QueryBuilder();
            var dir = Directory.GetCurrentDirectory();
            executor = new QueryExecutor(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=" + dir + @"\App_Data\TestData.mdf;Integrated Security=True;Connect Timeout=30");
        }

        [Test]
        public void SimpleSelectExecutesCorrectly()
        {
            var found = executor.LoadObjects(from user in builder.Table<User>() select user).First();
            Assert.That(found.Login, Is.EqualTo("jacenty"));
        }

        [Test]
        public void SelectWithWhereExecutesCorrectly()
        {
            var found = executor.LoadObjects(from user in builder.Table<User>() where user.FirstName == "Jacek" select user).Single();
            Assert.That(found.Login, Is.EqualTo("jacenty"));
        }

        [Test]
        public void StringFunctionsAreCalculatedCorrectly()
        {
            string j = "j";
            var user1 = executor.LoadObjects(from u in builder.Table<User>() where u.Login.StartsWith(j) select u).First();
            Assert.That(user1.Login, Is.EqualTo("jacenty"));

            var user2 = executor.LoadObjects(from u in builder.Table<User>() where u.Login.Contains("cent") select u).First();
            Assert.That(user2.Login, Is.EqualTo("jacenty"));

            var user3 = executor.LoadObjects(from u in builder.Table<User>() where u.Login.Substring(2, 4) == "cent" select u).First();
            Assert.That(user2.Login, Is.EqualTo("jacenty"));

            var result1 = executor.LoadScalars(from u in builder.Table<User>() where u.Login == "jacenty" select u.FirstName.Replace("J", "Pl")).First();
            Assert.That(result1, Is.EqualTo("Placek"));

            var result2 = executor.LoadScalars(from u in builder.Table<User>() select "  XYZ  ".TrimStart()).First();
            Assert.That(result2, Is.EqualTo("XYZ  "));

            var result3 = executor.LoadScalars(from u in builder.Table<User>() select "  XYZ  ".TrimEnd()).First();
            Assert.That(result3, Is.EqualTo("  XYZ"));

            var result4 = executor.LoadScalars(from u in builder.Table<User>() select "  XYZ  ".Trim()).First();
            Assert.That(result4, Is.EqualTo("XYZ"));

            var result5 = executor.LoadScalars(from u in builder.Table<User>() select "xyz".ToUpper()).First();
            Assert.That(result5, Is.EqualTo("XYZ"));

            var result6 = executor.LoadScalars(from u in builder.Table<User>() select "XYZ".ToLower()).First();
            Assert.That(result6, Is.EqualTo("xyz"));

            int result7 = executor.LoadScalars(from u in builder.Table<User>() select "XYZ".IndexOf("Y")).First();
            Assert.That(result7, Is.EqualTo(1));

            var result8 = executor.LoadScalars(from u in builder.Table<User>() select int.Parse("10")).First();
            Assert.That(result8, Is.EqualTo(10));

            var result9 = executor.LoadScalars(from u in builder.Table<User>() select long.Parse("10")).First();
            Assert.That(result9, Is.EqualTo(10L));

            var result10 = executor.LoadScalars(from u in builder.Table<User>() select double.Parse("10.0")).First();
            Assert.That(result10, Is.EqualTo(10.0));

            decimal result11 = executor.LoadScalars(from u in builder.Table<User>() select decimal.Parse("10.0")).First();
            Assert.That(result11, Is.EqualTo(10.0M));

            var result12 = executor.LoadScalars(from u in builder.Table<User>() select 10.ToString()).First();
            Assert.That(result12, Is.EqualTo("10"));

            var result13 = executor.LoadScalars(from u in builder.Table<User>() select 10L.ToString()).First();
            Assert.That(result13, Is.EqualTo("10"));

            var result14 = executor.LoadScalars(from u in builder.Table<User>() select 10.5.ToString()).First();
            Assert.That(result14, Is.EqualTo("10.5"));

            var result15 = executor.LoadScalars(from u in builder.Table<User>() select 10.5M.ToString()).First();
            Assert.That(result15, Is.EqualTo("10.5"));

            var result16 = executor.LoadScalars(from u in builder.Table<User>() select "jacenty".Length).First();
            Assert.That(result16, Is.EqualTo(7));
        }

        [Test]
        public void DateTimeFunctionsAreCalculatedCorrectly()
        {
            var result1 = executor.LoadScalars(from u in builder.Table<User>() select DateTime.Parse("2010/01/01")).First();
            Assert.That(result1, Is.EqualTo(new DateTime(2010, 1, 1)));

            var d1 = new DateTime(2010, 1, 1);
            var d2 = new DateTime(2010, 2, 1);
            var result2 = executor.LoadScalars(from u in builder.Table<User>() select d2.Subtract(d1).Days).First();
            Assert.That(result2, Is.EqualTo(31));

            var result3 = executor.LoadScalars(from u in builder.Table<User>() select d1.AddDays(31)).First();
            Assert.That(result3, Is.EqualTo(d2));

            var result4 = executor.LoadScalars(from u in builder.Table<User>() select d1.AddMonths(1)).First();
            Assert.That(result4, Is.EqualTo(d2));

            var result5 = executor.LoadScalars(from u in builder.Table<User>() select d1.AddYears(1)).First();
            Assert.That(result5, Is.EqualTo(new DateTime(2011, 1, 1)));

            var dt = new DateTime(2010, 1, 1, 10, 0, 0, 0);
            var result6 = executor.LoadScalars(from u in builder.Table<User>() select dt.AddHours(1)).First();
            Assert.That(result6, Is.EqualTo(new DateTime(2010, 1, 1, 11, 0, 0, 0)));

            var result7 = executor.LoadScalars(from u in builder.Table<User>() select dt.AddMinutes(1)).First();
            Assert.That(result7, Is.EqualTo(new DateTime(2010, 1, 1, 10, 1, 0, 0)));

            var result8 = executor.LoadScalars(from u in builder.Table<User>() select dt.AddSeconds(1)).First();
            Assert.That(result8, Is.EqualTo(new DateTime(2010, 1, 1, 10, 0, 1, 0)));

            var result9 = executor.LoadScalars(from u in builder.Table<User>() select dt.AddMilliseconds(10)).First();
            Assert.That(result9, Is.EqualTo(new DateTime(2010, 1, 1, 10, 0, 0, 10)));

            dt = new DateTime(2010, 10, 11, 12, 30, 15, 10);
            var result10 = executor.LoadScalars(from u in builder.Table<User>() select dt.Year).First();
            Assert.That(result10, Is.EqualTo(2010));

            var result11 = executor.LoadScalars(from u in builder.Table<User>() select dt.Month).First();
            Assert.That(result11, Is.EqualTo(10));

            var result12 = executor.LoadScalars(from u in builder.Table<User>() select dt.Day).First();
            Assert.That(result12, Is.EqualTo(11));

            var result13 = executor.LoadScalars(from u in builder.Table<User>() select dt.Hour).First();
            Assert.That(result13, Is.EqualTo(12));

            var result14 = executor.LoadScalars(from u in builder.Table<User>() select dt.Minute).First();
            Assert.That(result14, Is.EqualTo(30));

            var result15 = executor.LoadScalars(from u in builder.Table<User>() select dt.Second).First();
            Assert.That(result15, Is.EqualTo(15));

            var result16 = executor.LoadScalars(from u in builder.Table<User>() select dt.Millisecond).First();
            Assert.That(result16, Is.EqualTo(10));

            var result17 = executor.LoadScalars(from u in builder.Table<User>() select dt.DayOfYear).First();
            Assert.That(result17, Is.EqualTo(dt.DayOfYear));

            var result18 = executor.LoadScalars(from u in builder.Table<User>() select dt.DayOfWeek).First();
            Assert.That(result18, Is.EqualTo(dt.DayOfWeek));

            var now = DateTime.Now;
            var result19 = executor.LoadScalars(from u in builder.Table<User>() select DateTime.Now).First();
            Assert.That(result19, Is.LessThanOrEqualTo(DateTime.Now.AddMilliseconds(5)).And.GreaterThanOrEqualTo(now.AddMilliseconds(-5)));

            var utcNow = DateTime.UtcNow;
            var result20 = executor.LoadScalars(from u in builder.Table<User>() select DateTime.UtcNow).First();
            Assert.That(result20, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMilliseconds(5)).And.GreaterThanOrEqualTo(utcNow.AddMilliseconds(-5)));

            var result21 = executor.LoadScalars(from u in builder.Table<User>() select DateTime.Today).First();
            Assert.That(result21.Date, Is.EqualTo(DateTime.Today));

            d1 = new DateTime(2010, 1, 1);
            d2 = new DateTime(2010, 1, 3);
            var result22 = executor.LoadScalars(from u in builder.Table<User>() select d2.Subtract(d1).Days).First();
            Assert.That(result22, Is.EqualTo(2));

            var result23 = executor.LoadScalars(from u in builder.Table<User>() select d2.Subtract(d1).TotalDays).First();
            Assert.That(result23, Is.EqualTo(2));

            var result24 = executor.LoadScalars(from u in builder.Table<User>() select d2.Subtract(d1).TotalHours).First();
            Assert.That(result24, Is.EqualTo(48));

            var result25 = executor.LoadScalars(from u in builder.Table<User>() select d2.Subtract(d1).TotalMinutes).First();
            Assert.That(result25, Is.EqualTo(2880));

            var result26 = executor.LoadScalars(from u in builder.Table<User>() select d2.Subtract(d1).TotalSeconds).First();
            Assert.That(result26, Is.EqualTo(172800));

            var result27 = executor.LoadScalars(from u in builder.Table<User>() select d2.Subtract(d1).TotalMilliseconds).First();
            Assert.That(result27, Is.EqualTo(172800000));
        }

        [Test]
        public void MathFunctionsAreCalculatedCorrectly()
        {
            var result1 = executor.LoadScalars(from u in builder.Table<User>() select Math.Abs(-1)).First();
            Assert.That(result1, Is.EqualTo(1));
            var result2 = executor.LoadScalars(from u in builder.Table<User>() select Math.Abs(-1L)).First();
            Assert.That(result2, Is.EqualTo(1L));
            var result3 = executor.LoadScalars(from u in builder.Table<User>() select Math.Abs(-1.0F)).First();
            Assert.That(result3, Is.EqualTo(1.0F));
            var result4 = executor.LoadScalars(from u in builder.Table<User>() select Math.Abs(-1.0)).First();
            Assert.That(result4, Is.EqualTo(1.0));
            var result5 = executor.LoadScalars(from u in builder.Table<User>() select Math.Abs(-1.0M)).First();
            Assert.That(result5, Is.EqualTo(1.0M));
            
            var result6 = executor.LoadScalars(from u in builder.Table<User>() select Math.Acos(1)).First();
            Assert.That(result6, Is.EqualTo(0));

            var result7 = executor.LoadScalars(from u in builder.Table<User>() select Math.Asin(1)).First();
            Assert.That(result7, Is.EqualTo(Math.Asin(1)));

            var result8 = executor.LoadScalars(from u in builder.Table<User>() select Math.Atan(1)).First();
            Assert.That(result8, Is.EqualTo(Math.Atan(1)));

            var result9 = executor.LoadScalars(from u in builder.Table<User>() select Math.Atan2(0.5, 1)).First();
            Assert.That(result9, Is.EqualTo(Math.Atan2(0.5, 1)));

            var result10 = executor.LoadScalars(from u in builder.Table<User>() select Math.Ceiling(0.5M)).First();
            Assert.That(result10, Is.EqualTo(1));
            var result11 = executor.LoadScalars(from u in builder.Table<User>() select Math.Ceiling(0.5)).First();
            Assert.That(result11, Is.EqualTo(1));

            var result12 = executor.LoadScalars(from u in builder.Table<User>() select Math.Cos(0)).First();
            Assert.That(result12, Is.EqualTo(1));

            var result13 = executor.LoadScalars(from u in builder.Table<User>() select Math.Exp(0)).First();
            Assert.That(result13, Is.EqualTo(1));

            var result14 = executor.LoadScalars(from u in builder.Table<User>() select Math.Floor(0.5M)).First();
            Assert.That(result14, Is.EqualTo(0));
            var result15 = executor.LoadScalars(from u in builder.Table<User>() select Math.Floor(0.5)).First();
            Assert.That(result15, Is.EqualTo(0));

            var result16 = executor.LoadScalars(from u in builder.Table<User>() select Math.Log(1)).First();
            Assert.That(result16, Is.EqualTo(0));

            var result17 = executor.LoadScalars(from u in builder.Table<User>() select Math.Log10(1)).First();
            Assert.That(result17, Is.EqualTo(0));

            var result18 = executor.LoadScalars(from u in builder.Table<User>() select Math.Pow(2, 2)).First();
            Assert.That(result18, Is.EqualTo(4));

            var result19 = executor.LoadScalars(from u in builder.Table<User>() select Math.Round(0.55M, 1)).First();
            Assert.That(result19, Is.EqualTo(0.6));

            var result20 = executor.LoadScalars(from u in builder.Table<User>() select Math.Sign(-1)).First();
            Assert.That(result20, Is.EqualTo(-1));
            var result21 = executor.LoadScalars(from u in builder.Table<User>() select Math.Sign(-1L)).First();
            Assert.That(result21, Is.EqualTo(-1));
            var result22 = executor.LoadScalars(from u in builder.Table<User>() select Math.Sign(-1.0F)).First();
            Assert.That(result22, Is.EqualTo(-1));
            var result23 = executor.LoadScalars(from u in builder.Table<User>() select Math.Sign(-1.0)).First();
            Assert.That(result23, Is.EqualTo(-1));
            var result24 = executor.LoadScalars(from u in builder.Table<User>() select Math.Sign(-1.0M)).First();
            Assert.That(result24, Is.EqualTo(-1));

            var result25 = executor.LoadScalars(from u in builder.Table<User>() select Math.Sqrt(4)).First();
            Assert.That(result25, Is.EqualTo(2));

            var result26 = executor.LoadScalars(from u in builder.Table<User>() select Math.Tan(0.5)).First();
            Assert.That(result26, Is.EqualTo(Math.Tan(0.5)));

            var result27 = executor.LoadScalars(from u in builder.Table<User>() select new Random().NextDouble()).First();
            Assert.That(result27, Is.LessThanOrEqualTo(1).And.GreaterThanOrEqualTo(0));
            var result28 = executor.LoadScalars(from u in builder.Table<User>() select new Random().Next(5)).First();
            Assert.That(result28, Is.LessThanOrEqualTo(5).And.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void TypeCastsAreCalculatedCorrectly()
        {
            var x = 0.5;
            var result1 = executor.LoadScalars(from u in builder.Table<User>() select (decimal)x).First();
            Assert.That(result1, Is.EqualTo(0.5));
        }

        [Test]
        public void SkipExecutesCorrectly()
        {
            var result = executor.LoadObjects(builder.Table<User>().OrderBy(u => u.Login).Skip(1));
            Assert.That(result.Count(), Is.EqualTo(1));
        }
        
        [Test]
        public void TakeExecutesCorrectly()
        {
            var result = executor.LoadObjects(builder.Table<User>().OrderBy(u => u.Login).Take(1));
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void WhereOnBoolPropertyExecutesCorrectly()
        {
            var result = executor.LoadObjects(builder.Table<User>().Where(u => u.Disabled));
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void SkipAndTakeExecutesCorrectly()
        {
            var result = executor.LoadObjects(builder.Table<User>().OrderBy(u => u.Login).Skip(1).Take(1));
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
}
