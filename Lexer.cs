using System;
using System.Collections.Generic;
using System.Linq;

internal class Lexer
{
    private string text;
    private Position pos;
    private char currCh;
    private string fileName;

    public Lexer(string fileName, string text)
    {
        this.text = text;
        this.pos = new Position(-1, 0, -1, fileName, text);
        this.currCh = Constant.nullCh;
        Advance();
    }

    public void Advance()
    {
        pos.Advance(currCh);
        currCh = (pos.idx < text.Length) ? text[pos.idx] : Constant.nullCh;
    }

    public (List<Token> Tokens, Error Error) MakeTokens()
    {
        List<Token> tokens = new();

        while (currCh != Constant.nullCh)
        {
            if (" \t".Contains(currCh))
            {
                Advance();
            }
            if (";\n".Contains(currCh))
            {
                tokens.Add(new Token(Token.TT_NEWLINE, null, pos));
                Advance();
            }
            else if (Constant.DIGITS.Contains(currCh))
            {
                tokens.Add(MakeNumber());
            }
            else if (Constant.LETTERS.Contains(currCh))
            {
                tokens.Add(MakeIdentifier());
            }
            else if (currCh == '"')
            {
                tokens.Add(MakeString());
            }
            else if (currCh == '+')
            {
                tokens.Add(new Token(Token.TT_PLUS, null, pos));
                Advance();
            }
            else if (currCh == '-')
            {
                tokens.Add(MakeMinusOrArrow());
            }
            else if (currCh == '*')
            {
                tokens.Add(new Token(Token.TT_MUL, null, pos));
                Advance();
            }
            else if (currCh == '/')
            {
                tokens.Add(new Token(Token.TT_DIV, null, pos));
                Advance();
            }
            else if (currCh == '^')
            {
                tokens.Add(new Token(Token.TT_POW, null, pos));
                Advance();
            }
            else if (currCh == '(')
            {
                tokens.Add(new Token(Token.TT_LPAREN, null, pos));
                Advance();
            }
            else if (currCh == ')')
            {
                tokens.Add(new Token(Token.TT_RPAREN, null, pos));
                Advance();
            }
            else if (currCh == '[')
            {
                tokens.Add(new Token(Token.TT_LSQUARE, null, pos));
                Advance();
            }
            else if (currCh == ']')
            {
                tokens.Add(new Token(Token.TT_RSQUARE, null, pos));
                Advance();
            }
            else if (currCh == '!')
            {
                (Token token, Error error) = MakeNotEquals();
                if (error != null) return (null, error);
                tokens.Add(token);
            }
            else if (currCh == '=')
            {
                tokens.Add(MakeEquals());
            }
            else if (currCh == '<')
            {
                tokens.Add(MakeLessThan());
            }
            else if (currCh == '>')
            {
                tokens.Add(MakeGreaterThan());
            }
            else if (currCh == ',')
            {
                tokens.Add(new Token(Token.TT_COMMA, null, pos));
                Advance();
            }
            else
            {
                Position posStart = pos.Copy();
                char ch = currCh;
                Advance();
                return (null, new IllegalCharError("'" + ch + "'", posStart, pos));
            }

        }

        tokens.Add(new Token(Token.TT_EOF, null, pos));
        return (tokens, null);
    }

    private Token MakeString()
    {
        string s = "";
        Position posStart = pos.Copy();
        bool escapeCh = false;
        Advance();

        Dictionary<char, char> escapeCharacters = new()
        {
            { 'n', '\n' },
            { 't', '\t' }
        };

        while (currCh != Constant.nullCh && (currCh != '"' || escapeCh))
        {
            if (escapeCh)
            {
                char ch;
                _ = escapeCharacters.TryGetValue(currCh, out ch);
                if (ch == Constant.nullCh) ch = currCh;
                s += ch;
                escapeCh = false;
            }
            else if (currCh == '\\')
            {
                escapeCh = true;
            }
            else
            {
                s += currCh;
            }
            Advance();
        }

        Advance();
        return new Token(Token.TT_STRING, s, posStart, pos);
    }

    private Token MakeMinusOrArrow()
    {
        string tokenType = Token.TT_MINUS;
        Position posStart = pos.Copy();
        Advance();

        if (currCh == '>')
        {
            Advance();
            tokenType = Token.TT_ARROW;
        }
        return new Token(tokenType, posStart, pos);
    }

    private Token MakeNumber()
    {
        string numStr = "";
        int dotCount = 0;
        Position posStart = pos.Copy();

        while (currCh != Constant.nullCh && (Constant.DIGITS + ".").Contains(currCh))
        {
            if (currCh == '.')
            {
                if (dotCount == 1) break;
                dotCount++;
                numStr += ".";
            }
            else
            {
                numStr += currCh;
            }
            Advance();
        }

        if (dotCount == 0)
        {
            return new Token(Token.TT_INT, int.Parse(numStr), posStart, pos);
        }
        else
        {
            return new Token(Token.TT_FLOAT, float.Parse(numStr), posStart, pos);
        }
    }

    private Token MakeIdentifier()
    {
        string idStr = "";
        Position posStart = pos.Copy();

        while (currCh != Constant.nullCh && (Constant.LETTERS_DIGITS + '_').Contains(currCh))
        {
            idStr += currCh;
            Advance();
        }

        string tokenType = Token.KEYWORDS.Contains(idStr) ? Token.TT_KEYWORD : Token.TT_IDENTIFIER;
        return new Token(tokenType, idStr, posStart, pos);
    }

    private (Token token, Error error) MakeNotEquals()
    {
        Position posStart = pos.Copy();
        Advance();

        if (currCh == '=')
        {
            Advance();
            return (new Token(Token.TT_NE, null, posStart, pos), null);
        }

        Advance();
        return (null, new ExpectedCharError("'=' (after '!')", posStart, pos));
    }

    private Token MakeEquals()
    {
        Position posStart = pos.Copy();
        Advance();

        string tokenType = Token.TT_EQ;
        if (currCh == '=')
        {
            Advance();
            tokenType = Token.TT_EE;
        }

        return new Token(tokenType, null, posStart, pos);
    }

    private Token MakeLessThan()
    {
        Position posStart = pos.Copy();
        Advance();

        string tokenType = Token.TT_LT;
        if (currCh == '=')
        {
            Advance();
            tokenType = Token.TT_LTE;
        }

        return new Token(tokenType, null, posStart, pos);
    }

    private Token MakeGreaterThan()
    {
        Position posStart = pos.Copy();
        Advance();

        string tokenType = Token.TT_GT;
        if (currCh == '=')
        {
            Advance();
            tokenType = Token.TT_GTE;
        }

        return new Token(tokenType, null, posStart, pos);
    }

}
