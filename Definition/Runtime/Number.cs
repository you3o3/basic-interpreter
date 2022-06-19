using System;

internal class Number : RuntimeValue
{
    public static Number NULL = new(0);
    public static Number FALSE = new(0);
    public static Number TRUE = new(1);
    public static Number MATH_PI = new(Math.PI);

    internal new double value;

    public Number(double value)
    {
        this.value = value;
        base.value = value;
    }

    public override RuntimeValue Copy()
    {
        Number copy = new(value);
        copy.SetPos(posStart, posEnd);
        copy.SetContext(context);
        return copy;
    }

    //TODO could use covarient return type?
    internal override (RuntimeValue Value, Error Error) AddTo(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value + number.value).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) SubBy(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value - number.value).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) MulBy(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value * number.value).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) DivBy(RuntimeValue other)
    {
        if (other is Number number)
        {
            if (number.value == 0)
                return (null, new RuntimeError(
                    "Division by zero", context, other.posStart, other.posEnd
                ));
            return (new Number(this.value / number.value).SetContext(context), null);
        }
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) PowBy(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(Math.Pow(this.value, number.value)).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }


    internal override (RuntimeValue Value, Error Error) Comparison_eq(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value == number.value ? 1d : 0d).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) Comparison_ne(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value != number.value ? 1d : 0d).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) Comparison_lt(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value < number.value ? 1d : 0d).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) Comparison_gt(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value > number.value ? 1d : 0d).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) Comparison_lte(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value <= number.value ? 1d : 0d).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) Comparison_gte(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number(this.value >= number.value ? 1d : 0d).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }


    internal override (RuntimeValue Value, Error Error) AndBy(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number((this.value == 0d || number.value == 0d) ? 0d : 1d).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) OrBy(RuntimeValue other)
    {
        if (other is Number number)
            return (new Number((this.value != 0d || number.value != 0d) ? 1d : 0d).SetContext(context), null);
        else
            return (null, IncompatibleTypeOperation(other));
    }


    internal override (RuntimeValue Value, Error Error) Not()
    {
        return (new Number(value == 0d ? 1d : 0d), null);
    }

    internal override bool? IsTrue()
    {
        return value != 0d;
    }

    public override string ToString()
    {
        if (value % 1 == 0) return ((int)value).ToString();
        return value.ToString();
    }
}