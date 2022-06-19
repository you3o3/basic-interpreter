using System;
using System.Collections.Generic;
using System.Linq;

internal abstract class BaseFunction : RuntimeValue
{
    protected string name;

    public BaseFunction(string name)
    {
        this.name = name;
    }

    protected Context GenerateNewContext()
    {
        Context newContext = new(name, context, posStart);
        newContext.symbolTable = new(newContext.parent.symbolTable);
        return newContext;
    }

    protected RuntimeResult CheckArguments(List<string> argNames, RuntimeValue[] args)
    {
        RuntimeResult res = new();
        if (args.Length > argNames.Count)
        {
            return res.Failure(
                new RuntimeError(string.Format("{0} too many arguments passed into {1}", args.Length - argNames.Count, name), context, posStart, posEnd)
                );
        }
        if (args.Length < argNames.Count)
        {
            return res.Failure(
                new RuntimeError(string.Format("{0} too few arguments passed into {1}", argNames.Count - args.Length, name), context, posStart, posEnd)
                );
        }
        return res.Success(null);
    }

    // store the arguments into the symbolTable
    protected void PopulateArguments(List<string> argNames, RuntimeValue[] args, Context executionContext)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string argName = argNames[i];
            RuntimeValue argValue = args[i];
            //if (argValue == null) continue;

            argValue.SetContext(executionContext);
            executionContext.symbolTable.Set(argName, argValue);
        }
    }

    protected RuntimeResult CheckAndPopulateArguments(List<string> argNames, RuntimeValue[] args, Context executionContext)
    {
        RuntimeResult res = new();
        res.Register(CheckArguments(argNames, args));
        if (res.error != null) return res;
        PopulateArguments(argNames, args, executionContext);
        return res.Success(null);
    }

    public override string ToString()
    {
        return string.Format("<function {0}>", name);
    }

    protected class FunctionAttribute : Attribute
    {
        public List<string> argNames;

        public FunctionAttribute(params string[] argNames)
        {
            this.argNames = argNames.ToList();
        }
    }
}