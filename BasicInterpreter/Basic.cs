using System.Collections.Generic;

public class Basic 
{
    ///////////////////////////////////////////////////////////////////////////
    // Usage
    ///////////////////////////////////////////////////////////////////////////

    //TODO better documentation and comments

    /**
     * var a = 6
     * var b = var c = var d = 1
     * 5 + (var x = 8) => 13, x = 8
     * 
     * [1, 2, 3] + 4 => [1, 2, 3, 4]
     * [1, 2, 3] * [4, 5] => [1, 2, 3, 4, 5]
     * [1, 2, 3] - 0 => [2, 3]      (remove at index 0)
     * [1, 2, 3] - -1 => [1, 2]     (remove at index -1)
     * [1, 2, 3] / 0 => 1           (get at index 0)
     * [1, 2, 3] / -1 => 3          (get at index -1)
     * 
     */

    ///////////////////////////////////////////////////////////////////////////
    // Run
    ///////////////////////////////////////////////////////////////////////////

    public static SymbolTable globalSymbolTable = new();

    private static void InitSymbolTable()
    {
        globalSymbolTable.Set("null", Number.NULL);
        globalSymbolTable.Set("true", Number.TRUE);
        globalSymbolTable.Set("false", Number.FALSE);
        globalSymbolTable.Set("math_pi", Number.MATH_PI);

        globalSymbolTable.Set("print", BuiltInFunction.print);
        globalSymbolTable.Set("print_ret", BuiltInFunction.print_ret);
        globalSymbolTable.Set("input", BuiltInFunction.input);
        globalSymbolTable.Set("input_int", BuiltInFunction.input_int);
        globalSymbolTable.Set("clear", BuiltInFunction.clear);
        globalSymbolTable.Set("is_number", BuiltInFunction.is_number);
        globalSymbolTable.Set("is_string", BuiltInFunction.is_string);
        globalSymbolTable.Set("is_list", BuiltInFunction.is_list);
        globalSymbolTable.Set("is_function", BuiltInFunction.is_function);
        globalSymbolTable.Set("append", BuiltInFunction.append);
        globalSymbolTable.Set("pop", BuiltInFunction.pop);
        globalSymbolTable.Set("extend", BuiltInFunction.extend);
        globalSymbolTable.Set("len", BuiltInFunction.len);
        globalSymbolTable.Set("run", BuiltInFunction.run);
    }

    public static (object Obj, Error Error, Context context) Run(string fileName, string code, Context context = null)
    {
        InitSymbolTable();
        // tokenize the string
        Lexer lexer = new(fileName, code);
        (List<Token> tokens, Error error) = lexer.MakeTokens();
        if (error != null) return (null, error, null);

        // parse and allocate the tokens into an abstract syntax tree
        Parser parser = new(tokens);
        var ast = parser.Parse();
        if (ast.error != null) return (null, ast.error, null);

        // actually run the program, i.e. interpret the ast
        Interpreter interpreter = new();
        Context c = context ?? new("<program>");
        c.symbolTable = globalSymbolTable;
        RuntimeResult result = interpreter.Visit((Node)ast.node, c);

        return (result.value, result.error, c);
    }
}
