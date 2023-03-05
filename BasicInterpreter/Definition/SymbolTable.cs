using System.Collections.Generic;

// keep track of variable names and values
public class SymbolTable
{
    Dictionary<string, object> symbols = new(); // TODO map from string to runtimevalue?
    SymbolTable parent;

    public SymbolTable(SymbolTable parent = null)
    {
        this.parent = parent;
    }

    public object Get(string name)
    {
        symbols.TryGetValue(name, out object value);
        if (value == null && parent != null)
        {
            return parent.Get(name);
        }
        return value;
    }

    public void Set(string name, object value)
    {
        symbols[name] = value;
    }

    public void Remove(string name)
    {
        symbols.Remove(name);
    }
}
