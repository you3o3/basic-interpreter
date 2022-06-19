using System;
using System.Collections.Generic;

// parse result into abstract syntax tree
internal class Parser
{
    List<Token> tokens;
    Token currToken;
    int tokenIdx;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        tokenIdx = -1;
        Advance();
    }

    public Token Advance()
    {
        tokenIdx++;
        if (tokenIdx < tokens.Count)
        {
            currToken = tokens[tokenIdx];
        }
        //Debug.Log(currToken.type);
        return currToken;
    }

    public ParseResult Parse()
    {
        ParseResult res = Expr();
        if (res.error == null && currToken.type != Token.TT_EOF)
        {
            return res.Failure(new InvalidSyntaxError("Expected '+', '-', '*', '/'", currToken.posStart, currToken.posEnd));
        }
        return res;
    }

    /////////////////////////////////////////////////////////////////

    private object IfExpr()
    {
        ParseResult res = new();
        List<(Node, Node)> cases = new();
        Node elseCase = null;

        if (!currToken.Matches(Token.TT_KEYWORD, "if"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'if'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node condition = null;
        Node expr = null;
        ParseResult returnRes = null;

        ParseResult SaveNextConditionAndExpr()
        {
            condition = (Node)res.Register(Expr());
            if (res.error != null) return res;

            if (!currToken.Matches(Token.TT_KEYWORD, "then"))
            {
                return res.Failure(new InvalidSyntaxError("Expected 'then'", currToken.posStart, currToken.posEnd));
            }

            res.RegisterAdvancement();
            Advance();

            expr = (Node)res.Register(Expr());
            if (res.error != null) return res;
            cases.Add((condition, expr));

            return null;
        }

        returnRes = SaveNextConditionAndExpr();
        if (returnRes != null) return returnRes;

        while (currToken.Matches(Token.TT_KEYWORD, "elif"))
        {
            res.RegisterAdvancement();
            Advance();

            returnRes = SaveNextConditionAndExpr();
            if (returnRes != null) return returnRes;
        }

        if (currToken.Matches(Token.TT_KEYWORD, "else"))
        {
            res.RegisterAdvancement();
            Advance();

            elseCase = (Node)res.Register(Expr());
            if (res.error != null) return res;
        }

        return res.Success(new IfNode(cases, elseCase));
    }

    private object ForExpr()
    {
        ParseResult res = new();

        // for <identifier> = expr to expr (step expr) then expr

        if (!currToken.Matches(Token.TT_KEYWORD, "for"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'for'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        if (currToken.type != Token.TT_IDENTIFIER)
        {
            return res.Failure(new InvalidSyntaxError("Expected identifier", currToken.posStart, currToken.posEnd));
        }

        Token varName = currToken;
        res.RegisterAdvancement();
        Advance();

        if (currToken.type != Token.TT_EQ)
        {
            return res.Failure(new InvalidSyntaxError("Expected '='", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node startValNode = (Node)res.Register(Expr());
        if (res.error != null) return res;

        if (!currToken.Matches(Token.TT_KEYWORD, "to"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'to'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node endValNode = (Node)res.Register(Expr());
        if (res.error != null) return res;

        Node stepValNode = null;

        if (currToken.Matches(Token.TT_KEYWORD, "step"))
        {
            res.RegisterAdvancement();
            Advance();

            stepValNode = (Node)res.Register(Expr());
            if (res.error != null) return res;
        }

        if (!currToken.Matches(Token.TT_KEYWORD, "then"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'then'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node bodyNode = (Node)res.Register(Expr());
        if (res.error != null) return res;

        return res.Success(new ForNode(varName, startValNode, endValNode, stepValNode, bodyNode));
    }

    private object WhileExpr()
    {
        ParseResult res = new();

        // while expr then expr

        if (!currToken.Matches(Token.TT_KEYWORD, "while"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'while'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node conditionNode = (Node)res.Register(Expr());
        if (res.error != null) return res;

        if (!currToken.Matches(Token.TT_KEYWORD, "then"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'then'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node bodyNode = (Node)res.Register(Expr());
        if (res.error != null) return res;

        return res.Success(new WhileNode(conditionNode, bodyNode));
    }

    private object FuncDef()
    {
        ParseResult res = new();

        if (!currToken.Matches(Token.TT_KEYWORD, "function"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'function'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Token varNameToken = null;
        if (currToken.type == Token.TT_IDENTIFIER)
        {
            varNameToken = currToken;
            res.RegisterAdvancement();
            Advance();

            if (currToken.type != Token.TT_LPAREN)
            {
                return res.Failure(new InvalidSyntaxError("Expected '('", currToken.posStart, currToken.posEnd));
            }
        }
        else
        {
            if (currToken.type != Token.TT_LPAREN)
            {
                return res.Failure(new InvalidSyntaxError("Expected identifier or '('", currToken.posStart, currToken.posEnd));
            }
        }

        res.RegisterAdvancement();
        Advance();

        List<Token> argNameTokens = new();
        if (currToken.type == Token.TT_IDENTIFIER)
        {
            argNameTokens.Add(currToken.Copy());
            res.RegisterAdvancement();
            Advance();

            while (currToken.type == Token.TT_COMMA)
            {
                res.RegisterAdvancement();
                Advance();

                if (currToken.type != Token.TT_IDENTIFIER)
                {
                    return res.Failure(new InvalidSyntaxError("Expected 'identifier'", currToken.posStart, currToken.posEnd));
                }

                argNameTokens.Add(currToken.Copy());
                res.RegisterAdvancement();
                Advance();
            }

            if (currToken.type != Token.TT_RPAREN)
            {
                return res.Failure(new InvalidSyntaxError("Expected ',' or ')'", currToken.posStart, currToken.posEnd));
            }
        }
        else
        {
            if (currToken.type != Token.TT_RPAREN)
            {
                return res.Failure(new InvalidSyntaxError("Expected identifier or ')'", currToken.posStart, currToken.posEnd));
            }
        }

        res.RegisterAdvancement();
        Advance();

        if (currToken.type != Token.TT_ARROW)
        {
            return res.Failure(new InvalidSyntaxError("Expected '->'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node nodeToReturn = (Node)res.Register(Expr());
        if (res.error != null) return res;

        return res.Success(new FuncDefNode(varNameToken, argNameTokens, nodeToReturn));
    }

    public ParseResult Atom()
    {
        ParseResult res = new();
        Token token = currToken;

        // int/ float
        if (token.type == Token.TT_INT || token.type == Token.TT_FLOAT)
        {
            res.RegisterAdvancement();
            Advance();
            return res.Success(new NumberNode(token));
        }

        // string
        if (token.type == Token.TT_STRING)
        {
            res.RegisterAdvancement();
            Advance();
            return res.Success(new StringNode(token));
        }

        // varName
        else if (token.type == Token.TT_IDENTIFIER)
        {
            res.RegisterAdvancement();
            Advance();
            return res.Success(new VarAccessNode(token));
        }

        // (
        else if (token.type == Token.TT_LPAREN)
        {
            res.RegisterAdvancement();
            Advance();
            Node expr = (Node)res.Register(Expr());
            if (res.error != null) return res;
            if (currToken.type == Token.TT_RPAREN)
            {
                res.RegisterAdvancement();
                Advance();
                return res.Success(expr);
            }
            else
            {
                return res.Failure(new InvalidSyntaxError("Expected ')'", currToken.posStart, currToken.posEnd));
            }
        }

        // [
        else if (token.type == Token.TT_LSQUARE)
        {
            Node listExpr = (Node)res.Register(ListExpr());
            if (res.error != null) return res;
            return res.Success(listExpr);
        }

        // if
        else if (token.Matches(Token.TT_KEYWORD, "if"))
        {
            Node ifExpr = (Node)res.Register(IfExpr());
            if (res.error != null) return res;
            return res.Success(ifExpr);
        }

        // for
        else if (token.Matches(Token.TT_KEYWORD, "for"))
        {
            Node forExpr = (Node)res.Register(ForExpr());
            if (res.error != null) return res;
            return res.Success(forExpr);
        }

        // while
        else if (token.Matches(Token.TT_KEYWORD, "while"))
        {
            Node whileExpr = (Node)res.Register(WhileExpr());
            if (res.error != null) return res;
            return res.Success(whileExpr);
        }

        // function
        else if (token.Matches(Token.TT_KEYWORD, "function"))
        {
            Node funcDef = (Node)res.Register(FuncDef());
            if (res.error != null) return res;
            return res.Success(funcDef);
        }

        return res.Failure(new InvalidSyntaxError("Expected int, float, identifier, '+', '-', '(', '[', 'if', 'for', 'while' or 'function'", token.posStart, token.posEnd));
    }

    public ParseResult Power()
    {
        return BinOp(Call, new List<string> { Token.TT_POW }, Factor);
    }

    public ParseResult Call()
    {
        ParseResult res = new();
        Node atom = (Node)res.Register(Atom());
        if (res.error != null) return res;

        if (currToken.type == Token.TT_LPAREN)
        {
            res.RegisterAdvancement();
            Advance();
            List<Node> argNodes = new();

            if (currToken.type == Token.TT_RPAREN)
            {
                res.RegisterAdvancement();
                Advance();
            }
            else
            {
                argNodes.Add((Node)res.Register(Expr()));
                if (res.error != null)
                    return res.Failure(new InvalidSyntaxError(
                        "Expected ')', 'var', 'if', 'for', 'while', 'function', int, float, identifier, '+', '-', '(', '[' or 'not'",
                        currToken.posStart, currToken.posEnd
                    ));

                while (currToken.type == Token.TT_COMMA)
                {
                    res.RegisterAdvancement();
                    Advance();

                    argNodes.Add((Node)res.Register(Expr()));
                    if (res.error != null) return res;
                }

                if (currToken.type != Token.TT_RPAREN)
                {
                    return res.Failure(new InvalidSyntaxError("Expected ',' or ')'", currToken.posStart, currToken.posEnd));
                }

                res.RegisterAdvancement();
                Advance();
            }
            return res.Success(new CallNode(atom, argNodes));
        }
        return res.Success(atom);
    }

    public ParseResult Factor()
    {
        ParseResult res = new();
        Token token = currToken;

        if (token.type == Token.TT_PLUS || token.type == Token.TT_MINUS)
        {
            res.RegisterAdvancement();
            Advance();
            Node factor = (Node)res.Register(Factor());
            if (res.error != null) return res;
            return res.Success(new UnaryOpNode(factor, token));
        }
        return Power();
    }

    public ParseResult Term()
    {
        return BinOp(Factor, new List<string> { Token.TT_MUL, Token.TT_DIV });
    }

    public ParseResult ArithExpr()
    {
        return BinOp(Term, new List<string> { Token.TT_PLUS, Token.TT_MINUS });
    }

    public ParseResult ListExpr()
    {
        ParseResult res = new();
        List<Node> elementNodes = new();
        Position posStart = currToken.posStart.Copy();

        if (currToken.type != Token.TT_LSQUARE)
        {
            return res.Failure(new InvalidSyntaxError(
                "Expected '['",
                currToken.posStart, currToken.posEnd
            ));
        }

        res.RegisterAdvancement();
        Advance();

        if (currToken.type == Token.TT_RSQUARE)
        {
            res.RegisterAdvancement();
            Advance();
        }
        else
        {
            elementNodes.Add((Node)res.Register(Expr()));
            if (res.error != null)
                return res.Failure(new InvalidSyntaxError(
                    "Expected ']', 'var', 'if', 'for', 'while', 'function', int, float, identifier, '+', '-', '(', '[' or 'not'",
                    currToken.posStart, currToken.posEnd
                ));

            while (currToken.type == Token.TT_COMMA)
            {
                res.RegisterAdvancement();
                Advance();

                elementNodes.Add((Node)res.Register(Expr()));
                if (res.error != null) return res;
            }

            if (currToken.type != Token.TT_RSQUARE)
            {
                return res.Failure(new InvalidSyntaxError("Expected ',' or ']'", currToken.posStart, currToken.posEnd));
            }

            res.RegisterAdvancement();
            Advance();
        }
        return res.Success(new ListNode(elementNodes, posStart, currToken.posEnd.Copy()));
    }

    public ParseResult CompExpr()
    {
        ParseResult res = new();

        if (currToken.Matches(Token.TT_KEYWORD, "not"))
        {
            Token opToken = currToken;
            res.RegisterAdvancement();
            Advance();

            Node node_ = (Node)res.Register(CompExpr());
            if (res.error != null) return res;
            return res.Success(new UnaryOpNode(node_, opToken));
        }
        Node node = (Node)res.Register(BinOp(ArithExpr, new List<string>
                { Token.TT_EE, Token.TT_NE, Token.TT_LT, Token.TT_GT, Token.TT_LTE, Token.TT_GTE }
        ));
        if (res.error != null) return res.Failure(new InvalidSyntaxError(
            "Expected int, float, identifier, '+', '-', '(', '[' or 'not'",
            currToken.posStart, currToken.posEnd
        ));
        return res.Success(node);
    }

    public ParseResult Expr()
    {
        ParseResult res = new();
        if (currToken.Matches(Token.TT_KEYWORD, "var"))
        {
            res.RegisterAdvancement();
            Advance();

            if (currToken.type != Token.TT_IDENTIFIER)
            {
                return res.Failure(new InvalidSyntaxError("Expected identifier", currToken.posStart, currToken.posEnd));
            }

            Token varName = currToken;
            res.RegisterAdvancement();
            Advance();

            if (currToken.type != Token.TT_EQ)
            {
                return res.Failure(new InvalidSyntaxError("Expected '='", currToken.posStart, currToken.posEnd));
            }

            res.RegisterAdvancement();
            Advance();
            Node expr = (Node)res.Register(Expr());
            if (res.error != null) return res;
            return res.Success(new VarAssignNode(varName, expr));
        }

        Node node = (Node)res.Register(BinOp(CompExpr,
            new List<(string, string)> { (Token.TT_KEYWORD, "and"), (Token.TT_KEYWORD, "or") }));
        if (res.error != null)
            return res.Failure(new InvalidSyntaxError(
                "Expected 'var', 'if', 'for', 'while', 'function', int, float, identifier, '+', '-', '(', '[' or 'not'",
                currToken.posStart, currToken.posEnd
            ));

        return res.Success(node);
    }

    /////////////////////////////////////////////////////////////////

    public ParseResult BinOp(Func<ParseResult> func1, List<string> opTokens, Func<ParseResult> func2 = null)
    {
        if (func2 == null) func2 = func1;
        ParseResult res = new();
        Node left = (Node)res.Register(func1());
        if (res.error != null) return res;

        while (opTokens.Contains(currToken.type))
        {
            //Debug.Log("currentToken: " + currToken.type);
            //opTokens.ForEach((opToken) => { Debug.Log("opTokens: " + opToken); });
            Token opToken = currToken;
            res.RegisterAdvancement();
            Advance();
            Node right = (Node)res.Register(func2());
            if (res.error != null) return res;
            left = new BinOpNode(left, right, opToken);
        }
        return res.Success(left);
    }

    public ParseResult BinOp(Func<ParseResult> func1, List<(string TokenType, string Keyword)> opTokens, Func<ParseResult> func2 = null)
    {
        if (func2 == null) func2 = func1;

        ParseResult res = new();
        Node left = (Node)res.Register(func1());
        if (res.error != null) return res;

        //if (!(currToken.value is string))
        //{
        //    (_, List<string> keyword) = Unpack(opTokens);
        //    return res.Failure(
        //        new InvalidSyntaxError(string.Format("Expected {0}", string.Join(", ", keyword)),
        //        currToken.posStart, currToken.posEnd));
        //}

        bool IsContain()
        {
            foreach ((string tokenType, string keyword) in opTokens)
            {
                if (currToken.type == tokenType && (string)currToken.value == keyword)
                {
                    return true;
                }
            }
            return false;
        }

        while ((currToken.value is string) && IsContain())
        {
            //Debug.Log("BinOp");
            //Debug.Log("currentToken: " + currToken.type + (string)currToken.value);
            //opTokens.ForEach((opToken) => { Debug.Log("opTokens: " + opToken); });
            Token opToken = currToken;
            res.RegisterAdvancement();
            Advance();
            Node right = (Node)res.Register(func2());
            if (res.error != null) return res;
            left = new BinOpNode(left, right, opToken);
        }
        return res.Success(left);
    }

}