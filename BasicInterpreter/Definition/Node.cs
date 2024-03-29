﻿using System.Collections.Generic;

internal abstract class Node
{
    internal Position posStart, posEnd;
}

internal class NumberNode : Node
{
    internal Token token;

    public NumberNode(Token token)
    {
        this.token = token;
        posStart = token.posStart;
        posEnd = token.posEnd;
    }

    public override string ToString()
    {
        return token.ToString();
    }
}

internal class StringNode : Node
{
    internal Token token;

    public StringNode(Token token)
    {
        this.token = token;
        posStart = token.posStart;
        posEnd = token.posEnd;
    }

    public override string ToString()
    {
        return token.ToString();
    }
}

internal class ListNode : Node
{
    internal List<Node> elementNodes;

    public ListNode(List<Node> elementNodes, Position posStart, Position posEnd)
    {
        this.elementNodes = elementNodes;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

internal class VarAccessNode : Node
{
    internal Token varNameToken;

    public VarAccessNode(Token varNameToken)
    {
        this.varNameToken = varNameToken;
        posStart = varNameToken.posStart;
        posEnd = varNameToken.posEnd;
    }
}

internal class VarAssignNode : Node
{
    internal Token varNameToken;
    internal Node valueNode;

    public VarAssignNode(Token varNameToken, Node valueNode)
    {
        this.varNameToken = varNameToken;
        this.valueNode = valueNode;
        posStart = varNameToken.posStart;
        posEnd = varNameToken.posEnd;
    }
}

// binary operation node
internal class BinOpNode : Node
{
    internal Node left, right;
    internal Token opToken;

    public BinOpNode(Node left, Node right, Token opToken)
    {
        this.left = left;
        this.right = right;
        this.opToken = opToken;
        posStart = left.posStart;
        posEnd = right.posEnd;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1}, {2})", left.ToString(), opToken.ToString(), right.ToString());
    }
}

internal class UnaryOpNode : Node
{
    internal Node node;
    internal Token opToken;

    public UnaryOpNode(Node node, Token opToken)
    {
        this.node = node;
        this.opToken = opToken;
        posStart = opToken.posStart;
        posEnd = node.posEnd;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", opToken.ToString(), node.ToString());
    }
}

internal class IfNode : Node
{
    internal List<(Node Condition, Node Expr, bool? shouldReturnNull)> cases;
    internal (Node Expr, bool? shouldReturnNull) elseCase;

    public IfNode(List<(Node Condition, Node Expr, bool? shouldReturnNull)> cases, (Node Expr, bool? shouldReturnNull) elseCase)
    {
        this.cases = cases;
        this.elseCase = elseCase;
        posStart = cases[0].Condition.posStart;
        posEnd = (elseCase.Expr ?? cases[cases.Count - 1].Condition).posEnd; // coalesce
    }
}

internal class ForNode : Node
{
    internal Token varNameToken;
    internal Node startValNode, endValNode, stepValNode, bodyNode;
    internal bool? shouldReturnNull;

    public ForNode(Token varNameToken, Node startValNode, Node endValNode, Node stepValNode, Node bodyNode, bool? shouldReturnNull)
    {
        this.varNameToken = varNameToken;
        this.startValNode = startValNode;
        this.endValNode = endValNode;
        this.stepValNode = stepValNode;
        this.bodyNode = bodyNode;
        this.shouldReturnNull = shouldReturnNull;
        posStart = varNameToken.posStart;
        posEnd = bodyNode.posEnd;
    }
}

internal class WhileNode : Node
{
    internal Node conditionNode, bodyNode;
    internal bool? shouldReturnNull;

    public WhileNode(Node conditionNode, Node bodyNode, bool? shouldReturnNull)
    {
        this.conditionNode = conditionNode;
        this.bodyNode = bodyNode;
        this.shouldReturnNull = shouldReturnNull;
        posStart = conditionNode.posStart;
        posEnd = bodyNode.posEnd;
    }
}

internal class FuncDefNode : Node
{
    internal Token varNameToken;
    internal List<Token> argNameTokens;
    internal Node bodyNode;
    internal bool? shouldAutoReturn;

    public FuncDefNode(Token varNameToken, List<Token> argNameTokens, Node bodyNode, bool? shouldAutoReturn)
    {
        this.varNameToken = varNameToken;
        this.argNameTokens = argNameTokens;
        this.bodyNode = bodyNode;
        this.shouldAutoReturn = shouldAutoReturn;

        if (varNameToken != null)
        {
            posStart = varNameToken.posStart;
        }
        else if (argNameTokens.Count > 0)
        {
            posStart = argNameTokens[0].posStart;
        }
        else
        {
            posStart = bodyNode.posStart;
        }

        posEnd = bodyNode.posEnd;
    }
}

internal class CallNode : Node
{
    internal Node nodeToCall;
    internal List<Node> argNodes;

    public CallNode(Node nodeToCall, List<Node> argNodes)
    {
        this.nodeToCall = nodeToCall;
        this.argNodes = argNodes;

        posStart = nodeToCall.posStart;

        if (argNodes.Count > 0)
        {
            posEnd = argNodes[^1].posEnd;
        }
        else
        {
            posEnd = nodeToCall.posEnd;
        }
    }
}

internal class ReturnNode : Node
{
    internal Node nodeToReturn;

    public ReturnNode(Node nodeToReturn, Position posStart, Position posEnd)
    {
        this.nodeToReturn = nodeToReturn;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

internal class ContinueNode : Node
{
    public ContinueNode(Position posStart, Position posEnd)
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

internal class BreakNode : Node
{
    public BreakNode(Position posStart, Position posEnd)
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}