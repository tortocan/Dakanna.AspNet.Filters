using System.Collections.Generic;

namespace Dakanna.AspNet.Filters
{
    public class AplhaNumericLenghtComparer : IComparer<string>
    {
        private readonly string bindPrefix;

        public AplhaNumericLenghtComparer(string bindPrefix)
        {
            this.bindPrefix = bindPrefix;
        }

        public int Compare(string x, string y)
        {
            int lengthComparison = y.Length.CompareTo(x.Length);
            if (lengthComparison == 0)
            {
                if ((x.Length == 1 && char.IsNumber(x[0])) || (x.Length > bindPrefix.Length && char.IsNumber(x[bindPrefix.Length + 1])))
                {
                    return y.CompareTo(x);
                }
                return x.CompareTo(y);
            }
            else
            {
                return lengthComparison;
            }
        }
    }
}