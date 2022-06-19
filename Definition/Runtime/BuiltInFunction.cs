using System;
using System.Collections.Generic;
using System.Reflection;

internal class BuiltInFunction : BaseFunction
{
    public static BuiltInFunction print = new("print");
    public static BuiltInFunction print_ret = new("print_ret");
    public static BuiltInFunction input = new("input");
    public static BuiltInFunction input_int = new("input_int");
    public static BuiltInFunction clear = new("clear");
    public static BuiltInFunction is_number = new("is_number");
    public static BuiltInFunction is_string = new("is_string");
    public static BuiltInFunction is_list = new("is_list");
    public static BuiltInFunction is_function = new("is_function");
    public static BuiltInFunction append = new("append");
    public static BuiltInFunction pop = new("pop");
    public static BuiltInFunction extend = new("extend");

    public BuiltInFunction(string name) : base(name)
    {
        // built in function now
        // print(string value)
    }

    internal override RuntimeResult Execute(RuntimeValue[] args)
    {
        RuntimeResult res = new();
        Context executionContext = GenerateNewContext();

        string methodName = string.Format("Execute_{0}", name);
        MethodInfo method = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null) NoVisitMethod(executionContext);

        List<string> argNames = method.GetCustomAttribute<FunctionAttribute>().argNames;

        _ = res.Register(CheckAndPopulateArguments(argNames, args, executionContext));
        if (res.error != null) return res;

        //RuntimeResult returnValue = (RuntimeResult)res.Register(method.Invoke(this, new object[] { executionContext }));
        object returnValue = res.Register(method.Invoke(this, new object[] { executionContext }));
        if (res.error != null) return res;

        return res.Success(returnValue);
    }

    private void NoVisitMethod(Context context)
    {
        throw new Exception(string.Format("No Execute_{0} method defined", name));
    }

    public override RuntimeValue Copy()
    {
        BuiltInFunction copy = new(name);
        copy.SetPos(posStart, posEnd);
        copy.SetContext(context);
        return copy;
    }

    public override string ToString()
    {
        return string.Format("<built-in function {0}>", name);
    }

    // printing the value
    [Function("value")]
    private RuntimeResult Execute_print(Context executionContext)
    {
        Console.WriteLine(executionContext.symbolTable.Get("value").ToString());
        return new RuntimeResult().Success(Number.NULL);
    }

    // return the value instead of printing it
    [Function("value")]
    private RuntimeResult Execute_print_ret(Context executionContext)
    {
        return new RuntimeResult().Success(new String(executionContext.symbolTable.Get("value").ToString()));
    }

    // take input from user
    [Function()]
    private RuntimeResult Execute_input(Context executionContext)
    {
        string input = Console.ReadLine();
        return new RuntimeResult().Success(new String(input));
    }

    // take integer input from user
    [Function()]
    private RuntimeResult Execute_input_int(Context executionContext)
    {
        int result;
        while (true)
        {
            string input = Console.ReadLine();
            bool success = int.TryParse(input, out result);
            if (success)
            {
                break;
            }
            else
            {
                Console.WriteLine(string.Format("{0} must be an integer. Try again!", input));
            }
        }
        return new RuntimeResult().Success(new Number(result));
    }

    // clear screen
    [Function()]
    private RuntimeResult Execute_clear(Context executionContext)
    {
        Console.Clear();
        return new RuntimeResult().Success(Number.NULL);
    }

    [Function("value")]
    private RuntimeResult Execute_is_number(Context executionContext)
    {
        bool result = executionContext.symbolTable.Get("value") is Number;
        return new RuntimeResult().Success(result ? Number.TRUE : Number.FALSE);
    }

    [Function("value")]
    private RuntimeResult Execute_is_string(Context executionContext)
    {
        bool result = executionContext.symbolTable.Get("value") is String;
        return new RuntimeResult().Success(result ? Number.TRUE : Number.FALSE);
    }

    [Function("value")]
    private RuntimeResult Execute_is_list(Context executionContext)
    {
        bool result = executionContext.symbolTable.Get("value") is List;
        return new RuntimeResult().Success(result ? Number.TRUE : Number.FALSE);
    }

    [Function("value")]
    private RuntimeResult Execute_is_function(Context executionContext)
    {
        bool result = executionContext.symbolTable.Get("value") is BaseFunction;
        return new RuntimeResult().Success(result ? Number.TRUE : Number.FALSE);
    }

    [Function("list", "value")]
    private RuntimeResult Execute_append(Context executionContext)
    {
        object listTest = executionContext.symbolTable.Get("list");
        RuntimeValue value = (RuntimeValue)executionContext.symbolTable.Get("value");

        if (listTest is not List)
        {
            return new RuntimeResult().Failure(new RuntimeError("First argument must be list"
                , executionContext, posStart, posEnd));
        }
        List list = (List)listTest;

        list.elements.Add(value);
        return new RuntimeResult().Success(Number.NULL);
    }

    [Function("list", "index")]
    private RuntimeResult Execute_pop(Context executionContext)
    {
        object listTest = executionContext.symbolTable.Get("list");
        if (listTest is not List)
        {
            return new RuntimeResult().Failure(new RuntimeError("First argument must be list"
                , executionContext, posStart, posEnd));
        }
        List list = (List)listTest;

        object indexTest = executionContext.symbolTable.Get("index");
        if (indexTest is not Number)
        {
            return new RuntimeResult().Failure(new RuntimeError("Second argument must be number"
                , executionContext, posStart, posEnd));
        }
        Number index = (Number)indexTest;

        object element;
        try
        {
            element = list.elements[(int)index.value];
            list.elements.RemoveAt((int)index.value);
        }
        catch (Exception)
        {
            return new RuntimeResult().Failure(
                new RuntimeError("Element at this index could not be removed from list because index is out of bounds"
                , executionContext, posStart, posEnd));
        }
        return new RuntimeResult().Success(element);
    }

    [Function("listA", "listB")]
    private RuntimeResult Execute_extend(Context executionContext)
    {
        object listATest = executionContext.symbolTable.Get("listA");
        if (listATest is not List)
        {
            return new RuntimeResult().Failure(new RuntimeError("First argument must be list"
                , executionContext, posStart, posEnd));
        }
        List listA = (List)listATest;

        object listBTest = executionContext.symbolTable.Get("listB");
        if (listBTest is not List)
        {
            return new RuntimeResult().Failure(new RuntimeError("Second argument must be list"
                , executionContext, posStart, posEnd));
        }
        List listB = (List)listBTest;

        listA.elements.AddRange(listB.elements);
        return new RuntimeResult().Success(Number.NULL);
    }
}
