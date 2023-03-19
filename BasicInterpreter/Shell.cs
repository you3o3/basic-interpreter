using System;

class Shell
{
    private static Context context;
    private static volatile bool running = true;

    static void Main(string[] args)
    {
        Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Shell.running = false;
        };

        while (running)
        {
            Console.Write("basic > ");
            var text = Console.ReadLine();
            if (text == null || text.Trim() == "")
            {
                continue;
            }

            (object obj, Error err, Context context) = Basic.Run("Shell.cs", text, Shell.context);
            if (err != null)
            {
                Console.WriteLine(err.ToString());
            }
            else
            {
                Console.WriteLine(obj.ToString());
                Shell.context = context;
            }
        }
    }
}
