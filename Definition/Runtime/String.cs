using System.Linq;

internal class String : RuntimeValue
{
    internal new string value;

    public String(string value)
    {
        this.value = value;
        base.value = value;
    }

    public override RuntimeValue Copy()
    {
        String copy = new(value);
        copy.SetPos(posStart, posEnd);
        copy.SetContext(context);
        return copy;
    }

    internal override (RuntimeValue Value, Error Error) AddTo(RuntimeValue other)
    {
        if (other is String s)
            return (new String(this.value + s.value).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) MulBy(RuntimeValue other)
    {
        if (other is Number number)
            return (new String(string.Concat(Enumerable.Repeat(this.value, (int)number.value))).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override bool? IsTrue()
    {
        return value.Length > 0;
    }

    //FIXME the print function would print with "", see last part in ep 11.

    public override string ToString()
    {
        return string.Format("\"{0}\"", value);
    }
}
