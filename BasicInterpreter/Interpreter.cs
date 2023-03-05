using System;
using System.Collections.Generic;
using System.Reflection;

internal class Interpreter
{
    public RuntimeResult Visit(Node node, Context context)
    {
        string methodName = string.Format("Visit_{0}", node.GetType().Name);
        MethodInfo method = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null) NoVisitMethod(node, context);
        object[] args = new object[] { node, context };
        return (RuntimeResult)method.Invoke(this, args);
    }

    private void NoVisitMethod(Node node, Context context)
    {
        throw new Exception(string.Format("No Visit_{0} defined", node.GetType().Name));
    }

    private RuntimeResult Visit_NumberNode(Node node, Context context)
    {
        NumberNode numberNode = (NumberNode)node;
        Token token = numberNode.token;
        return new RuntimeResult().Success(
            new Number(double.Parse(token.value.ToString())).SetPos(token.posStart, token.posEnd)
        );
    }

    private RuntimeResult Visit_StringNode(Node node, Context context)
    {
        StringNode stringNode = (StringNode)node;
        Token token = stringNode.token;
        return new RuntimeResult().Success(
            new String(token.value.ToString()).SetPos(token.posStart, token.posEnd)
        );
    }

    private RuntimeResult Visit_ListNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        ListNode listNode = (ListNode)node;

        List<RuntimeValue> elements = new();
        foreach (Node elementNode in listNode.elementNodes)
        {
            elements.Add(res.Register<RuntimeValue>(Visit(elementNode, context)));
            if (res.ShouldReturn()) return res;
        }
        return res.Success(new List(elements).SetContext(context).SetPos(listNode.posStart, listNode.posEnd));
    }

    private RuntimeResult Visit_VarAccessNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        VarAccessNode varAccessNode = (VarAccessNode)node;
        string varName = (string)varAccessNode.varNameToken.value;
        RuntimeValue value = (RuntimeValue)context.symbolTable.Get(varName);

        if (value == null)
        {
            return res.Failure(new RuntimeError(
                string.Format("{0} is not defined", varName), context, node.posStart, node.posEnd));
        }
        value = value.Copy().SetPos(node.posStart, node.posEnd).SetContext(context);
        return res.Success(value);
    }

    private RuntimeResult Visit_VarAssignNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        VarAssignNode varAssignNode = (VarAssignNode)node;
        string varName = (string)varAssignNode.varNameToken.value;
        var value = res.Register<RuntimeValue>(Visit(varAssignNode.valueNode, context));
        if (res.ShouldReturn()) return res;

        context.symbolTable.Set(varName, value);
        return res.Success(value);
    }

    private RuntimeResult Visit_BinOpNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        BinOpNode binOpNode = (BinOpNode)node;
        Token token = binOpNode.opToken;

        RuntimeValue left = res.Register<RuntimeValue>(Visit(binOpNode.left, context));
        if (res.ShouldReturn()) return res;
        RuntimeValue right = res.Register<RuntimeValue>(Visit(binOpNode.right, context));
        if (res.ShouldReturn()) return res;

        (RuntimeValue Value, Error Error) result = (null, null);

        if (token.type == Token.TT_PLUS) result = left.AddTo(right);
        else if (token.type == Token.TT_MINUS) result = left.SubBy(right);
        else if (token.type == Token.TT_MUL) result = left.MulBy(right);
        else if (token.type == Token.TT_DIV) result = left.DivBy(right);
        else if (token.type == Token.TT_POW) result = left.PowBy(right);

        else if (token.type == Token.TT_EE) result = left.Comparison_eq(right);
        else if (token.type == Token.TT_NE) result = left.Comparison_ne(right);
        else if (token.type == Token.TT_LT) result = left.Comparison_lt(right);
        else if (token.type == Token.TT_GT) result = left.Comparison_gt(right);
        else if (token.type == Token.TT_LTE) result = left.Comparison_lte(right);
        else if (token.type == Token.TT_GTE) result = left.Comparison_gte(right);

        else if (token.Matches(Token.TT_KEYWORD, "and")) result = left.AndBy(right);
        else if (token.Matches(Token.TT_KEYWORD, "or")) result = left.OrBy(right);

        if (result.Error != null)
            return res.Failure(result.Error);
        return res.Success(result.Value.SetPos(token.posStart, token.posEnd));
    }

    private RuntimeResult Visit_UnaryOpNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        UnaryOpNode unaryOpNode = (UnaryOpNode)node;
        Token token = unaryOpNode.opToken;

        RuntimeValue num = res.Register<RuntimeValue>(Visit(unaryOpNode.node, context));
        if (res.ShouldReturn()) return res;

        (RuntimeValue Value, Error Error) result = (null, null);

        if (token.type == Token.TT_MINUS)
        {
            result = num.MulBy(new Number(-1));
        }
        else if (token.Matches(Token.TT_KEYWORD, "not"))
        {
            result = num.Not();
        }

        if (result.Error != null)
            return res.Failure(result.Error);
        return res.Success(result.Value.SetPos(token.posStart, token.posEnd));
    }

    private RuntimeResult Visit_IfNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        IfNode ifNode = (IfNode)node;

        foreach ((Node condition, Node expr, bool? shouldReturnNull) in ifNode.cases)
        {
            RuntimeValue conditionValue = res.Register<RuntimeValue>(Visit(condition, context));
            if (res.ShouldReturn()) return res;

            if (conditionValue.IsTrue() ?? false)
            {
                RuntimeValue exprValue = res.Register<RuntimeValue>(Visit(expr, context));
                if (res.ShouldReturn()) return res;
                return res.Success(shouldReturnNull == null || shouldReturnNull == true ? Number.NULL : exprValue);
            }
        }

        if (ifNode.elseCase.Expr != null)
        {
            RuntimeValue elseValue = res.Register<RuntimeValue>(Visit(ifNode.elseCase.Expr, context));
            if (res.ShouldReturn()) return res;
            return res.Success(ifNode.elseCase.shouldReturnNull == null || ifNode.elseCase.shouldReturnNull == true ? Number.NULL : elseValue);
        }

        return res.Success(Number.NULL);
    }

    private RuntimeResult Visit_ForNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        ForNode forNode = (ForNode)node;

        List<RuntimeValue> elements = new();

        RuntimeValue startValue = res.Register<RuntimeValue>(Visit(forNode.startValNode, context));
        if (res.ShouldReturn()) return res;

        RuntimeValue endValue = res.Register<RuntimeValue>(Visit(forNode.endValNode, context));
        if (res.ShouldReturn()) return res;

        RuntimeValue stepValue = new Number(1);
        if (forNode.stepValNode != null)
        {
            stepValue = res.Register<RuntimeValue>(Visit(forNode.stepValNode, context));
            if (res.ShouldReturn()) return res;
        }

        if (!(startValue is Number && endValue is Number && stepValue is Number))
        {
            return res.Failure(new RuntimeError("Incompatible type", context, node.posStart, node.posEnd));
        }

        double i = ((Number)startValue).value;
        double endVal = ((Number)endValue).value;
        double stepVal = ((Number)stepValue).value;
        Func<bool> condition = () => { return i > endVal; };
        if (stepVal >= 0)
        {
            condition = () => { return i < endVal; };
        }

        while (condition())
        {
            context.symbolTable.Set((string)forNode.varNameToken.value, new Number(i));
            i += stepVal;

            RuntimeValue value = res.Register<RuntimeValue>(Visit(forNode.bodyNode, context));
            if (res.ShouldReturn() && res.loopShouldContinue == false && res.loopShouldBreak == false) return res;

            if (res.loopShouldContinue) continue;
            if (res.loopShouldBreak) break;

            elements.Add(value);
        }

        return res.Success(
            (bool)forNode.shouldReturnNull ? Number.NULL :
            new List(elements).SetContext(context).SetPos(forNode.posStart, forNode.posEnd)
        );
    }

    private RuntimeResult Visit_WhileNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        WhileNode whileNode = (WhileNode)node;

        List<RuntimeValue> elements = new();

        while (true)
        {
            RuntimeValue condition = res.Register<RuntimeValue>(Visit(whileNode.conditionNode, context));
            if (res.ShouldReturn()) return res;

            if (!(condition.IsTrue() ?? false)) break;

            RuntimeValue value = res.Register<RuntimeValue>(Visit(whileNode.bodyNode, context));

            if (res.ShouldReturn() && res.loopShouldContinue == false && res.loopShouldBreak == false) return res;

            if (res.loopShouldContinue) continue;
            if (res.loopShouldBreak) break;

            elements.Add(value);
        }

        return res.Success(
            (bool)whileNode.shouldReturnNull ? Number.NULL :
            new List(elements).SetContext(context).SetPos(whileNode.posStart, whileNode.posEnd)
        );
    }

    private RuntimeResult Visit_FuncDefNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        FuncDefNode funcDefNode = (FuncDefNode)node;

        string funcName = funcDefNode.varNameToken != null ? (string)funcDefNode.varNameToken.value : null;

        Node bodyNode = funcDefNode.bodyNode;

        List<string> argNames = new();
        foreach (Token argName in funcDefNode.argNameTokens)
        {
            argNames.Add((string)argName.value);
        }

        Function funcVal = new(funcName, bodyNode, argNames, funcDefNode.shouldAutoReturn);
        funcVal.SetContext(context);
        funcVal.SetPos(funcDefNode.posStart, funcDefNode.posEnd);

        if (funcDefNode.varNameToken != null)
        {
            context.symbolTable.Set(funcName, funcVal);
        }
        return res.Success(funcVal);
    }

    private RuntimeResult Visit_CallNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        CallNode callNode = (CallNode)node;

        RuntimeValue[] args = new RuntimeValue[callNode.argNodes.Count];

        RuntimeValue valueToCall = res.Register<RuntimeValue>(Visit(callNode.nodeToCall, context));
        if (res.ShouldReturn()) return res;
        valueToCall = valueToCall.Copy();
        valueToCall.SetPos(callNode.posStart, callNode.posEnd);

        for (int i = 0; i < callNode.argNodes.Count; i++)
        {
            Node argNode = callNode.argNodes[i];
            args[i] = res.Register<RuntimeValue>(Visit(argNode, context));
            if (res.ShouldReturn()) return res;
        }
        //foreach (Node argNode in callNode.argNodes)
        //{
        //    args.Append(res.Register<RuntimeValue>(Visit(argNode, context)));
        //    if (res.ShouldReturn()) return res;
        //}

        RuntimeValue returnVal = res.Register<RuntimeValue>(valueToCall.Execute(args));
        if (res.ShouldReturn()) return res;
        returnVal = returnVal.SetPos(node.posStart, node.posEnd).SetContext(context);
        return res.Success(returnVal);
    }

    private RuntimeResult Visit_ReturnNode(Node node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        ReturnNode returnNode = (ReturnNode)node;
        RuntimeValue value = Number.NULL;

        if (returnNode.nodeToReturn != null)
        {
            value = res.Register<RuntimeValue>(Visit(returnNode.nodeToReturn, context));
            if (res.ShouldReturn()) return res;
        }

        return res.SuccessReturn(value);
    }

    private RuntimeResult Visit_ContinueNode(Node node, Context context)
    {
        return new RuntimeResult().SuccessContinue();
    }

    private RuntimeResult Visit_BreakNode(Node node, Context context)
    {
        return new RuntimeResult().SuccessBreak();
    }
}