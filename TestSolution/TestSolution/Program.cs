using System;

namespace TestSolutoin.Fieldm123
{
    class Program
    {
        public static int FieldTest1, Fieldm12;
        public static int Field3;
        public static int tux;
        public static string MultipleWordsInThis { get; set; }

        // TODO: Do not check  static constructors to see if they are called
        static Program()
        {
            FieldTest1 = 3;
            Fieldm12 = 5;
            Field3 = 4;
        }

        static void Main(string[] args)
        {
            int i = 3;
            MutateValue(i);

            Func<int, int> f = j => 5;
        }

        public event EventHandler TheEvent;
        public struct TheStruct { }
        public enum TheEnum { Item1, Item2, Item3 }

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
            i >>= 3;
            i ^= 3;
            i <<= 3;
            i++;
            i--;
            ++i;
            --i;

            string abcd, efgh, ijkl;

            // all changes to j should be fine
            int j = i;
            j += 3;
            j -= 3;
            j *= 3;
            j /= 3;
            j %= 3;
            j &= 3;
            j |= 3;
            j >>= 3;
            j ^= 3;
            j <<= 3;
            j++;
            j--;
            --j;
            ++j;
        }

        /// <summary>
        /// Refactoring should remove this comment
        /// </summary>
        internal static void NotCalled(int i)
        {

        }
    }
}