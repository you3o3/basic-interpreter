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

    private void UpdateCurrentToken()
    {
        if (tokenIdx >= 0 && tokenIdx < tokens.Count)
        {
            currToken = tokens[tokenIdx];
        }
    }

    public Token Advance()
    {
        tokenIdx++;
        UpdateCurrentToken();
        return currToken;
    }

    public Token Reverse(int amount = 1)
    {
        tokenIdx -= amount;
        UpdateCurrentToken();
        return currToken;
    }

    public ParseResult Parse()
    {
        ParseResult res = Statements();
        if (res.error == null && currToken.type != Token.TT_EOF)
        {
            return res.Failure(new InvalidSyntaxError(
                "Expected '+', '-', '*', '/', '^', '==', '!=', '<', '>', <=', '>=', 'and' or 'or'"
                , currToken.posStart, currToken.posEnd)
            );
        }
        return res;
    }

    /////////////////////////////////////////////////////////////////

    private ParseResult IfExprCases(string caseKeyword)
    {
        ParseResult res = new();
        List<(Node condition, Node statements, bool? shouldReturnNull)> cases = new();
        (Node, bool? shouldReturnNull) elseCase = (null, null);

        if (!currToken.Matches(Token.TT_KEYWORD, caseKeyword))
        {
            return res.Failure(new InvalidSyntaxError(
                string.Format("Expected '{0}'", caseKeyword), currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node condition = res.Register<Node>(Expr());
        if (res.error != null) return res;

        if (!currToken.Matches(Token.TT_KEYWORD, "then"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'then'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        if (currToken.type == Token.TT_NEWLINE)
        {
            res.RegisterAdvancement();
            Advance();

            Node statements = res.Register<Node>(Statements());
            if (res.error != null) return res;
            cases.Add((condition, statements, true));

            if (currToken.Matches(Token.TT_KEYWORD, "end"))
            {
                res.RegisterAdvancement();
                Advance();
            }
            else
            {
                var allCases = res.Register<(List<(Node, Node, bool?)>, (Node, bool?))>(IfExprB_OR_C());
                if (res.error != null) return res;
                List<(Node condition, Node statements, bool? shouldReturnNull)> newCases;
                (newCases, elseCase) = allCases;
                cases.AddRange(newCases);
            }
        }
        else
        {
            Node expr = res.Register<Node>(Statement());
            if (res.error != null) return res;
            cases.Add((condition, expr, false));

            var allCases = res.Register<(List<(Node, Node, bool?)>, (Node, bool?))>(IfExprB_OR_C());
            if (res.error != null) return res;
            List<(Node condition, Node statements, bool? shouldReturnNull)> newCases;
            (newCases, elseCase) = allCases;
            cases.AddRange(newCases);
        }
        return res.Success((cases, elseCase));
    } 

    private ParseResult IfExpr()
    {
        ParseResult res = new();
        (var cases, var elseCase) = res.Register<(List<(Node, Node, bool?)>, (Node, bool?))>(IfExprCases("if"));
        if (res.error != null) return res;
        return res.Success(new IfNode(cases, elseCase));
    }

    // elif
    private ParseResult IfExprB()
    {
        return IfExprCases("elif");
    }

    // else
    private ParseResult IfExprC()
    {
        ParseResult res = new();
        (Node, bool? shouldReturnNull) elseCase = (null, null);

        if (currToken.Matches(Token.TT_KEYWORD, "else"))
        {
            res.RegisterAdvancement();
            Advance();

            if (currToken.type == Token.TT_NEWLINE)
            {
                res.RegisterAdvancement();
                Advance();

                Node statements = res.Register<Node>(Statements());
                if (res.error != null) return res;
                elseCase = (statements, true);

                if (currToken.Matches(Token.TT_KEYWORD, "end"))
                {
                    res.RegisterAdvancement();
                    Advance();
                }
                else
                {
                    return res.Failure(new InvalidSyntaxError("Expected 'end'", currToken.posStart, currToken.posEnd));
                }
            }
            else
            {
                Node expr = res.Register<Node>(Statement());
                if (res.error != null) return res;
                elseCase = (expr, false);
            }
        }
        return res.Success(elseCase);
    }

    private ParseResult IfExprB_OR_C()
    {
        ParseResult res = new();
        List<(Node condition, Node statements, bool? shouldReturnNull)> cases = new();
        (Node, bool? shouldReturnNull) elseCase = (null, null);

        if (currToken.Matches(Token.TT_KEYWORD, "elif"))
        {
            var allCases = res.Register<(List<(Node, Node, bool?)>, (Node, bool ?))>(IfExprB());
            if (res.error != null) return res;
            (cases, elseCase) = allCases;
        }
        else
        {
            elseCase = res.Register<(Node, bool ?)>(IfExprC());
            if (res.error != null) return res;
        }

        return res.Success((cases, elseCase));
    }

    private ParseResult ForExpr()
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

        Node startValNode = res.Register<Node>(Expr());
        if (res.error != null) return res;

        if (!currToken.Matches(Token.TT_KEYWORD, "to"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'to'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node endValNode = res.Register<Node>(Expr());
        if (res.error != null) return res;

        Node stepValNode = null;

        if (currToken.Matches(Token.TT_KEYWORD, "step"))
        {
            res.RegisterAdvancement();
            Advance();

            stepValNode = res.Register<Node>(Expr());
            if (res.error != null) return res;
        }

        if (!currToken.Matches(Token.TT_KEYWORD, "then"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'then'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node body;
        if (currToken.type == Token.TT_NEWLINE)
        {
            res.RegisterAdvancement();
            Advance();

            body = res.Register<Node>(Statements());
            if (res.error != null) return res;

            if (!currToken.Matches(Token.TT_KEYWORD, "end"))
            {
                return res.Failure(new InvalidSyntaxError("Expected 'end'", currToken.posStart, currToken.posEnd));
            }

            res.RegisterAdvancement();
            Advance();

            return res.Success(new ForNode(varName, startValNode, endValNode, stepValNode, body, true));
        }

        body = res.Register<Node>(Statement());
        if (res.error != null) return res;

        return res.Success(new ForNode(varName, startValNode, endValNode, stepValNode, body, false));
    }

    private ParseResult WhileExpr()
    {
        ParseResult res = new();

        // while expr then expr

        if (!currToken.Matches(Token.TT_KEYWORD, "while"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'while'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node conditionNode = res.Register<Node>(Expr());
        if (res.error != null) return res;

        if (!currToken.Matches(Token.TT_KEYWORD, "then"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'then'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node body;
        if (currToken.type == Token.TT_NEWLINE)
        {
            res.RegisterAdvancement();
            Advance();

            body = res.Register<Node>(Statements());
            if (res.error != null) return res;

            if (!currToken.Matches(Token.TT_KEYWORD, "end"))
            {
                return res.Failure(new InvalidSyntaxError("Expected 'end'", currToken.posStart, currToken.posEnd));
            }

            res.RegisterAdvancement();
            Advance();

            return res.Success(new WhileNode(conditionNode, body, true));
        }

        body = res.Register<Node>(Statement());
        if (res.error != null) return res;

        return res.Success(new WhileNode(conditionNode, body, false));
    }

    private ParseResult FuncDef()
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

        if (currToken.type == Token.TT_ARROW)
        {
            res.RegisterAdvancement();
            Advance();

            Node nodeToReturn = res.Register<Node>(Expr());
            if (res.error != null) return res;

            return res.Success(new FuncDefNode(varNameToken, argNameTokens, nodeToReturn, true));
        }

        if (currToken.type != Token.TT_NEWLINE)
        {
            return res.Failure(new InvalidSyntaxError("Expected '->' or newline", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        Node body = res.Register<Node>(Statements());
        if (res.error != null) return res;

        if (!currToken.Matches(Token.TT_KEYWORD, "end"))
        {
            return res.Failure(new InvalidSyntaxError("Expected 'end'", currToken.posStart, currToken.posEnd));
        }

        res.RegisterAdvancement();
        Advance();

        return res.Success(new FuncDefNode(varNameToken, argNameTokens, body, false));
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
            Node expr = res.Register<Node>(Expr());
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
            Node listExpr = res.Register<Node>(ListExpr());
            if (res.error != null) return res;
            return res.Success(listExpr);
        }

        // if
        else if (token.Matches(Token.TT_KEYWORD, "if"))
        {
            Node ifExpr = res.Register<Node>(IfExpr());
            if (res.error != null) return res;
            return res.Success(ifExpr);
        }

        // for
        else if (token.Matches(Token.TT_KEYWORD, "for"))
        {
            Node forExpr = res.Register<Node>(ForExpr());
            if (res.error != null) return res;
            return res.Success(forExpr);
        }

        // while
        else if (token.Matches(Token.TT_KEYWORD, "while"))
        {
            Node whileExpr = res.Register<Node>(WhileExpr());
            if (res.error != null) return res;
            return res.Success(whileExpr);
        }

        // function
        else if (token.Matches(Token.TT_KEYWORD, "function"))
        {
            Node funcDef = res.Register<Node>(FuncDef());
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
        Node atom = res.Register<Node>(Atom());
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
                argNodes.Add(res.Register<Node>(Expr()));
                if (res.error != null)
                    return res.Failure(new InvalidSyntaxError(
                        "Expected ')', 'var', 'if', 'for', 'while', 'function', int, float, identifier, '+', '-', '(', '[' or 'not'",
                        currToken.posStart, currToken.posEnd
                    ));

                while (currToken.type == Token.TT_COMMA)
                {
                    res.RegisterAdvancement();
                    Advance();

                    argNodes.Add(res.Register<Node>(Expr()));
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
            Node factor = res.Register<Node>(Factor());
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
            elementNodes.Add(res.Register<Node>(Expr()));
            if (res.error != null)
                return res.Failure(new InvalidSyntaxError(
                    "Expected ']', 'var', 'if', 'for', 'while', 'function', int, float, identifier, '+', '-', '(', '[' or 'not'",
                    currToken.posStart, currToken.posEnd
                ));

            while (currToken.type == Token.TT_COMMA)
            {
                res.RegisterAdvancement();
                Advance();

                elementNodes.Add(res.Register<Node>(Expr()));
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

            Node node_ = res.Register<Node>(CompExpr());
            if (res.error != null) return res;
            return res.Success(new UnaryOpNode(node_, opToken));
        }
        Node node = res.Register<Node>(BinOp(ArithExpr, new List<string>
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
            Node expr = res.Register<Node>(Expr());
            if (res.error != null) return res;
            return res.Success(new VarAssignNode(varName, expr));
        }

        Node node = res.Register<Node>(BinOp(CompExpr,
            new List<(string, string)> { (Token.TT_KEYWORD, "and"), (Token.TT_KEYWORD, "or") }));
        if (res.error != null)
            return res.Failure(new InvalidSyntaxError(
                "Expected 'var', 'if', 'for', 'while', 'function', int, float, identifier, '+', '-', '(', '[' or 'not'",
                currToken.posStart, currToken.posEnd
            ));

        return res.Success(node);
    }

    public ParseResult Statements()
    {
        ParseResult res = new();
        List<Node> statements = new();
        Position posStart = currToken.posStart.Copy();

        while (currToken.type == Token.TT_NEWLINE)
        {
            res.RegisterAdvancement();
            Advance();
        }

        Node statement = res.Register<Node>(Statement());

        if (res.error != null) return res;
        statements.Add(statement);

        bool moreStatements = true;

        while (true)
        {
            int newlineCount = 0;
            while (currToken.type == Token.TT_NEWLINE)
            {
                res.RegisterAdvancement();
                Advance();
                newlineCount++;
            }

            if (newlineCount == 0)
            {
                moreStatements = false;
            }
            if (!moreStatements) break;

            Node s = res.TryRegister<Node>(Statement());

            if (s == default(Node))
            {
                Reverse(res.toReverseCount);
                moreStatements = false;
                continue;
            }
            statements.Add(s);
        }
        return res.Success(new ListNode(statements, posStart, currToken.posEnd.Copy()));
    }

    public ParseResult Statement()
    {
        ParseResult res = new();
        Position posStart = currToken.posStart.Copy();

        // return
        if (currToken.Matches(Token.TT_KEYWORD, "return"))
        {
            res.RegisterAdvancement();
            Advance();

            Node expr = res.TryRegister<Node>(Expr());
            if (expr == default(Node))
            {
                Reverse(res.toReverseCount);
            }

            return res.Success(new ReturnNode(expr, posStart, currToken.posStart.Copy()));
        }

        // continue
        if (currToken.Matches(Token.TT_KEYWORD, "continue"))
        {
            res.RegisterAdvancement();
            Advance();
            return res.Success(new ContinueNode(posStart, currToken.posStart.Copy()));
        }

        // break
        if (currToken.Matches(Token.TT_KEYWORD, "break"))
        {
            res.RegisterAdvancement();
            Advance();
            return res.Success(new BreakNode(posStart, currToken.posStart.Copy()));
        }

        Node expr2 = res.Register<Node>(Expr());
        if (res.error != null)
        {
            return res.Failure(new InvalidSyntaxError(
                "Expected 'return', 'continue', 'break', 'var', 'if', 'for', 'while', 'function', int, float, identifier, '+', '-', '(', '[' or 'not'",
                currToken.posStart, currToken.posEnd
            ));
        }
        return res.Success(expr2);
    }

    /////////////////////////////////////////////////////////////////

    public ParseResult BinOp(Func<ParseResult> func1, List<string> opTokens, Func<ParseResult> func2 = null)
    {
        if (func2 == null) func2 = func1;
        ParseResult res = new();
        Node left = res.Register<Node>(func1());
        if (res.error != null) return res;

        while (opTokens.Contains(currToken.type))
        {
            Token opToken = currToken;
            res.RegisterAdvancement();
            Advance();
            Node right = res.Register<Node>(func2());
            if (res.error != null) return res;
            left = new BinOpNode(left, right, opToken);
        }
        return res.Success(left);
    }

    public ParseResult BinOp(Func<ParseResult> func1, List<(string TokenType, string Keyword)> opTokens, Func<ParseResult> func2 = null)
    {
        if (func2 == null) func2 = func1;

        ParseResult res = new();
        Node left = res.Register<Node>(func1());
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
            Token opToken = currToken;
            res.RegisterAdvancement();
            Advance();
            Node right = res.Register<Node>(func2());
            if (res.error != null) return res;
            left = new BinOpNode(left, right, opToken);
        }
        return res.Success(left);
    }

}
