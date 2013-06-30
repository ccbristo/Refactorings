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
            i = 3;
        }

        /// <summary>
        /// Refactoring should remove this comment.
        /// </summary>
        internal static void NotCalled(int i)
        {
            
        }
    }
}
