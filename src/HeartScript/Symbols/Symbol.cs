namespace HeartScript.Ast
{
    public class Symbol<T> : ISymbol<T>
    {
        public bool IsPublic { get; }
        public T Value { get; }

        public Symbol(bool isPublic, T value)
        {
            IsPublic = isPublic;
            Value = value;
        }
    }
}
