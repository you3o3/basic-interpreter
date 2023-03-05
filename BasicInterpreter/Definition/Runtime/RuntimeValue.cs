internal abstract class RuntimeValue
{
    internal object value;
    internal Position posStart, posEnd;
    internal Context context;

    public virtual RuntimeValue SetPos(Position posStart = null, Position posEnd = null)
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
        return this;
    }

    internal virtual RuntimeValue SetContext(Context context)
    {
        this.context = context;
        return this;
    }

    public abstract RuntimeValue Copy();

    internal virtual (RuntimeValue Value, Error Error) AddTo(RuntimeValue other) { return DefaultOperatorError(other, '+'); }
    internal virtual (RuntimeValue Value, Error Error) SubBy(RuntimeValue other) { return DefaultOperatorError(other, '-'); }
    internal virtual (RuntimeValue Value, Error Error) MulBy(RuntimeValue other) { return DefaultOperatorError(other, '*'); }
    internal virtual (RuntimeValue Value, Error Error) DivBy(RuntimeValue other) { return DefaultOperatorError(other, '/'); }
    internal virtual (RuntimeValue Value, Error Error) PowBy(RuntimeValue other) { return DefaultOperatorError(other, '^'); }

    internal virtual (RuntimeValue Value, Error Error) Comparison_eq(RuntimeValue other) { return DefaultOperatorError(other, "=="); }
    internal virtual (RuntimeValue Value, Error Error) Comparison_ne(RuntimeValue other) { return DefaultOperatorError(other, "!="); }
    internal virtual (RuntimeValue Value, Error Error) Comparison_lt(RuntimeValue other) { return DefaultOperatorError(other, '<'); }
    internal virtual (RuntimeValue Value, Error Error) Comparison_gt(RuntimeValue other) { return DefaultOperatorError(other, '>'); }
    internal virtual (RuntimeValue Value, Error Error) Comparison_lte(RuntimeValue other) { return DefaultOperatorError(other, "<="); }
    internal virtual (RuntimeValue Value, Error Error) Comparison_gte(RuntimeValue other) { return DefaultOperatorError(other, ">="); }

    internal virtual (RuntimeValue Value, Error Error) AndBy(RuntimeValue other) { return DefaultKeywordError(other, "and"); }
    internal virtual (RuntimeValue Value, Error Error) OrBy(RuntimeValue other) { return DefaultKeywordError(other, "or"); }

    internal virtual (RuntimeValue Value, Error Error) Not() { return DefaultKeywordError(this, "not"); }

    internal virtual RuntimeResult Execute(RuntimeValue[] args) { return new RuntimeResult().Failure(IllegalOperation("Illegal operation")); }

    internal virtual bool? IsTrue() { return null; }

    private (RuntimeValue Value, Error Error) DefaultOperatorError(RuntimeValue other, char op)
    {
        return (null, IllegalOperation(string.Format("'{0}' operator is not defined for {1}", op, this.GetType().Name), other));
    }

    private (RuntimeValue Value, Error Error) DefaultOperatorError(RuntimeValue other, string op)
    {
        return (null, IllegalOperation(string.Format("'{0}' operator is not defined for {1}", op, this.GetType().Name), other));
    }

    private (RuntimeValue Value, Error Error) DefaultKeywordError(RuntimeValue other, string keyword)
    {
        return (null, IllegalOperation(string.Format("'{0}' keyword is not defined for {1}", keyword, this.GetType().Name), other));
    }

    private protected Error IllegalOperation(string message, RuntimeValue other = null)
    {
        if (other == null) other = this;
        return new RuntimeError(message, context, posStart, other.posEnd);
    }

    private protected Error IncompatibleTypeOperation(RuntimeValue other = null)
    {
        return IllegalOperation(string.Format("Incompatible type from {0} to {1}",
            (other != null) ? other.GetType().Name : "null", this.GetType().Name), other);
    }
}
