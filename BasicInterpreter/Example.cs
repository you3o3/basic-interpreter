using System;
using System.Reflection;

class Example
{
    static void Main(string[] args)
    {
        // The output should be:

        // "Hello world!"
        // "Greetings universe!"
        // "loop, spoop"
        // "loop, spoop"
        // "loop, spoop"
        // "loop, spoop"
        // "loop, spoop"
        // [0, 0]

        string path = Path.GetFullPath("example.myopl");
        (object obj, Error err, Context context) = Basic.Run("Hi", string.Format("print(\"Hello world!\"); run(\"{0}\")", path));
        if (err != null)
        {
            Console.WriteLine(err.ToString());
        }
        else
        {
            Console.WriteLine(obj.ToString());
        }
    }
}
