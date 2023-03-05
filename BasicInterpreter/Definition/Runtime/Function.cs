using System.Collections.Generic;

internal class Function : BaseFunction
{
    internal Node bodyNode;
    internal List<string> argNames;
    internal bool? shouldAutoReturn;

    public Function(string name, Node bodyNode, List<string> argNames, bool? shouldAutoReturn) : base(name)
    {
        this.bodyNode = bodyNode;
        this.argNames = argNames;
        this.shouldAutoReturn = shouldAutoReturn;
    }

    public override RuntimeValue Copy()
    {
        Function copy = new(name, bodyNode, argNames, shouldAutoReturn);
        copy.SetPos(posStart, posEnd);
        copy.SetContext(context);
        return copy;
    }

    internal override RuntimeResult Execute(RuntimeValue[] args)
    {
        RuntimeResult res = new();
        Interpreter interpreter = new();
        Context executionContext = GenerateNewContext();

        res.Register<object>(CheckAndPopulateArguments(argNames, args, context));
        if (res.ShouldReturn()) return res;

        RuntimeValue value = res.Register<RuntimeValue>(interpreter.Visit(bodyNode, executionContext));
        if (res.ShouldReturn() && res.funcReturnValue == null) return res;

        object returnValue = (bool)shouldAutoReturn ? value : null;
        if (returnValue == null) returnValue = res.funcReturnValue;
        if (returnValue == null) returnValue = Number.NULL;

        return res.Success(returnValue);
    }
}