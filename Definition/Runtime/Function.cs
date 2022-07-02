using System.Collections.Generic;

internal class Function : BaseFunction
{
    internal Node bodyNode;
    internal List<string> argNames;
    internal bool? shouldReturnNull;

    public Function(string name, Node bodyNode, List<string> argNames, bool? shouldReturnNull) : base(name)
    {
        this.bodyNode = bodyNode;
        this.argNames = argNames;
        this.shouldReturnNull = shouldReturnNull;
    }

    public override RuntimeValue Copy()
    {
        Function copy = new(name, bodyNode, argNames, shouldReturnNull);
        copy.SetPos(posStart, posEnd);
        copy.SetContext(context);
        return copy;
    }

    internal override RuntimeResult Execute(RuntimeValue[] args)
    {
        RuntimeResult res = new();
        Interpreter interpreter = new();
        Context executionContext = GenerateNewContext();

        res.Register(CheckAndPopulateArguments(argNames, args, context));
        if (res.error != null) return res;

        RuntimeValue value = (RuntimeValue)res.Register(interpreter.Visit(bodyNode, executionContext));
        if (res.error != null) return res;
        // TODO Number.NULL cannot be used here
        return (bool)shouldReturnNull ? null : res.Success(value);
    }
}