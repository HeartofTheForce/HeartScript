namespace HeartScript.Ast
{
    public interface ISymbol
    {
        bool IsPublic { get; }
    }

    public interface ISymbol<T> : ISymbol
    {
        T Value { get; }
    }
}
