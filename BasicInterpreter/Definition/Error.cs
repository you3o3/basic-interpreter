using static Helper;

public class Error
{
    private protected string name;
    private protected string details;
    private protected Position posStart;
    private protected Position posEnd;

    internal Error(string name, string details, Position posStart, Position posEnd)
    {
        this.name = name;
        this.details = details;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }

    public override string ToString()
    {
        string result = string.Format("{0}: {1}", name, details);
        result += string.Format("\nFile {0}, line {1}", posStart.fileName, posStart.line + 1);
        result += "\n\n" + stringWithArrows(posStart.fileText, posStart, posEnd);
        return result;
    }
}

public class IllegalCharError : Error
{
    internal IllegalCharError(string details, Position posStart, Position posEnd)
        : base("Illegal Character", details, posStart, posEnd) { }
}

public class ExpectedCharError : Error
{
    internal ExpectedCharError(string details, Position posStart, Position posEnd)
        : base("Expected Character", details, posStart, posEnd) { }
}

public class InvalidSyntaxError : Error
{
    internal InvalidSyntaxError(string details, Position posStart, Position posEnd)
        : base("Invalid syntax", details, posStart, posEnd) { }
}

public class RuntimeError : Error
{
    private Context context;

    internal RuntimeError(string details, Context context, Position posStart, Position posEnd)
        : base("Runtime error", details, posStart, posEnd)
    {
        this.context = context;
    }

    public override string ToString()
    {
        string result = GenerateTrackback();
        result += string.Format("{0}: {1}", name, details);
        result += "\n\n" + stringWithArrows(posStart.fileText, posStart, posEnd);
        return result;
    }

    private string GenerateTrackback()
    {
        string result = "";
        Context ctx = context;
        Position pos = posStart;

        while (context != null)
        {
            if (pos == null)
            {
                //FIXME temporary fix, sometimes pos is null and raise exception
                return result;
            }
            result = string.Format("    File {0}, line {1}, in {2}\n", pos.fileName, pos.line + 1, ctx.displayName) + result;
            pos = ctx.parentEntryPos;
            ctx = ctx.parent;
        }
        return "Traceback (most recent call last):\n" + result;
    }
}
