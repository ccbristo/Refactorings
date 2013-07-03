using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 3;
            MutateValue(i);

            Func<int, int> f = j => 5;
        }

        private static void MutateValue(int i)
        {
            // all changes to i should cause errors
            i = 3;
            i += 3;
            i -= 3;
            i *= 3;
            i /= 3;
            i %= 3;
            i &= 3;
            i |= 3;
            i++;
            i--;
            ++i;
            --i;

            // all changes to j should be fine
            int j = i;
            j += 3;
            j -= 3;
            j *= 3;
            j /= 3;
            j %= 3;
            j &= 3;
            j |= 3;
            j++;
            j--;
            --j;
            ++j;
        }

        /// <summary>
        /// Refactoring should remove this comment.
        /// </summary>
        internal static void NotCalled(int i)
        {
            
        }
    }
}