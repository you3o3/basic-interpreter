public class Context
{
    internal string displayName;
    internal Context parent;
    internal Position parentEntryPos;
    internal SymbolTable symbolTable;

    public Context(string displayName, Context parent = null, Position parentEntryPos = null)
    {
        this.displayName = displayName;
        this.parent = parent;
        this.parentEntryPos = parentEntryPos;
    }
}