using System.Collections.Generic;

internal class Function : BaseFunction
{
    Node bodyNode;
    List<string> argNames;

    public Function(string name, Node bodyNode, List<string> argNames) : base(name)
    {
        this.bodyNode = bodyNode;
        this.argNames = argNames;
    }

    public override RuntimeValue Copy()
    {
        Function copy = new(name, bodyNode, argNames);
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
        return res.Success(value);
    }
}