internal class Token
{
    // token types
    public const string TT_INT = "INT";
    public const string TT_FLOAT = "FLOAT";
    public const string TT_STRING = "STRING";
    public const string TT_PLUS = "PLUS";
    public const string TT_MINUS = "MINUS";
    public const string TT_MUL = "MUL";
    public const string TT_DIV = "DIV";
    public const string TT_POW = "POW";
    public const string TT_LPAREN = "LPAREN";
    public const string TT_RPAREN = "RPAREN";
    public const string TT_LSQUARE = "LSQUARE";
    public const string TT_RSQUARE = "RSQUARE";
    public const string TT_IDENTIFIER = "IDENTIFIER";
    public const string TT_KEYWORD = "KEYWORD";
    public const string TT_EQ = "EQ";
    public const string TT_COMMA = "COMMA";
    public const string TT_ARROW = "ARROW";
    public const string TT_EOF = "EOF";

    public const string TT_EE = "EE";  // equal
    public const string TT_NE = "NE";  // not equal
    public const string TT_LT = "LT";  // less than
    public const string TT_GT = "GT";  // greater than
    public const string TT_LTE = "LTE"; // less than or equal
    public const string TT_GTE = "GTE"; // greater than or equal
    public static readonly string[] KEYWORDS = new string[]
    {
            "var",
            "and", "or", "not",
            "if", "then", "elif", "else",
            "for", "to", "step", "while",
            "function"
    };

    public string type;
    public object value;
    public Position posStart, posEnd;

    public Token(string type, object value = null, Position posStart = null, Position posEnd = null)
    {
        this.type = type;
        this.value = value;

        if (posStart != null)
        {
            this.posStart = posStart.Copy();
            this.posEnd = posStart.Copy();
            this.posEnd.Advance();
        }
        if (posEnd != null) this.posEnd = posEnd;
    }

    public bool Matches(string type, object value)
    {
        return this.type.Equals(type) && this.value.Equals(value);
    }

    public override string ToString()
    {
        if (value != null) return string.Format("{0}:{1}", type, value);
        return type;
    }

    public Token Copy()
    {
        return new(type, value, posStart, posEnd);
    }
}
