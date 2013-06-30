namespace TestSolution
{
    public class UnnecessaryDefaultConstructor
    {
        private UnnecessaryDefaultConstructor() { }
    }

    public class RequiresDefaultConstructor
    {
        public RequiresDefaultConstructor() { }

        protected RequiresDefaultConstructor(int arg)
        {
            int a = arg; // just to use it
        }
    }

    public class X
    {
        internal class Y
        {
            public class Z
            {
                public Z() { }
            }
        }
    }
}
