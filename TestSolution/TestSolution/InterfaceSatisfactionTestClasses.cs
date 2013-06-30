using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSolution
{
    class UncalledMethodToSatisfyExternalInterface<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }
    }

    interface IControlThisOne
    {
        void Foo(int i);
    }

    class FooNotCalled : IControlThisOne
    {
        void IControlThisOne.Foo(int i)
        {
            throw new NotImplementedException();
        }
    }

    public class FooCalled : IControlThisOne
    {
        // this method is public, so it is assumed to be called from somewhere else
        public void Foo(int i)
        {
            int j = i + 5;
        }
    }
}
