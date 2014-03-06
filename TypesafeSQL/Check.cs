using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The utility class for checking method input parameters.
    /// </summary>
    public class Check
    {
        public static void NotNull(object value, string name)
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }

        public static void NotNullNorEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Argument can not be null or empty.", name);
        }

        internal static void NotNegative(int number, string name)
        {
            if (number < 0)
                throw new ArgumentException("Argument can not be negative number.", name);
        }
    }
}
