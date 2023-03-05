using System;
using System.Collections;
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
    public static BuiltInFunction len = new("len");
    public static BuiltInFunction run = new("run");

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

        _ = res.Register<object>(CheckAndPopulateArguments(argNames, args, executionContext));
        if (res.ShouldReturn()) return res;

        object returnValue = res.Register<object>((RuntimeResult)method.Invoke(this, new object[] { executionContext }));
        if (res.ShouldReturn()) return res;

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
        executionContext.symbolTable.Set("list", list);
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
        executionContext.symbolTable.Set("list", list);
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

        executionContext.symbolTable.Set("listA", listA);
        executionContext.symbolTable.Set("listB", listB);
        return new RuntimeResult().Success(Number.NULL);
    }

    [Function("list")]
    private RuntimeResult Execute_len(Context executionContext)
    {
        object listTest = executionContext.symbolTable.Get("list");
        if (listTest is not List)
        {
            return new RuntimeResult().Failure(new RuntimeError("Argument must be list"
                , executionContext, posStart, posEnd));
        }
        List list = (List)listTest;

        return new RuntimeResult().Success(new Number(list.elements.Count));
    }

    [Function("fn")]
    private RuntimeResult Execute_run(Context executionContext)
    {
        object fileNameTest = executionContext.symbolTable.Get("fn");
        if (fileNameTest is not String)
        {
            return new RuntimeResult().Failure(new RuntimeError("First argument must be string"
                , executionContext, posStart, posEnd));
        }
        string fileName = ((String)fileNameTest).value;

        string script;
        try
        {
            script = System.IO.File.ReadAllText(fileName);
        }
        catch (Exception)
        {
            return new RuntimeResult().Failure(new RuntimeError(string.Format("Failed to load script \"{0}\"", fileName)
                , executionContext, posStart, posEnd));
        }

        Error error;
        (_, error, _) = Basic.Run(fileName, script);

        if (error != null)
        {
            return new RuntimeResult().Failure(new RuntimeError(
                string.Format("Failed to finish executing script \"{0}\"\n{1}", fileName, error.ToString())
                , executionContext, posStart, posEnd));
        }

        return new RuntimeResult().Success(Number.NULL);
    }
}
