using System;

namespace HeartScript.UTests.End2EndTests
{
    public struct End2EndTestCase<T>
    {
        public string Infix { get; set; }
        public string ExpectedNodeString { get; set; }
        public Func<Context<T>, T> ExpectedFunction { get; set; }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }

    public struct Context<T>
    {
        public T A { get; set; }
        public T B { get; set; }
        public T C { get; set; }
        public T D { get; set; }
        public T E { get; set; }
        public T F { get; set; }
        public T G { get; set; }
        public T H { get; set; }
        public T I { get; set; }
    }
}
