using System;
using System.Collections.Generic;

internal class List : RuntimeValue
{
    internal List<RuntimeValue> elements;

    public List(List<RuntimeValue> elements)
    {
        this.elements = elements;
    }

    public override RuntimeValue Copy()
    {
        List<RuntimeValue> elementsCopy = new(elements.Count);
        elements.ForEach((item) =>
        {
            //elementsCopy.Add(item.Copy());
            elementsCopy.Add(item);
        });
        List copy = new(elementsCopy);
        copy.SetPos(posStart, posEnd);
        copy.SetContext(context);
        return copy;
    }

    internal override (RuntimeValue Value, Error Error) AddTo(RuntimeValue other)
    {
        List newList = (List)Copy();
        newList.elements.Add(other);
        return (newList, null);
    }

    internal override (RuntimeValue Value, Error Error) SubBy(RuntimeValue other)
    {
        if (other is Number number)
        {
            List newList = (List)Copy();
            try
            {
                newList.elements.RemoveAt((int)number.value);
                return (newList, null);
            }
            catch (Exception)
            {
                return (null, new RuntimeError(
                    "Element at this index could not be removed because index is out of bounds"
                    , context, other.posStart, other.posEnd)
                );
            }
        }
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) MulBy(RuntimeValue other)
    {
        if (other is List list)
        {
            List newList = (List)Copy();
            newList.elements.AddRange(list.elements);
            return (newList, null);
        }
        else
            return (null, IncompatibleTypeOperation(other));
    }

    internal override (RuntimeValue Value, Error Error) DivBy(RuntimeValue other)
    {
        if (other is Number number)
        {
            try
            {
                return (elements[(int)number.value], null);
            }
            catch (Exception)
            {
                return (null, new RuntimeError(
                    "Element at this index could not be retrieved because index is out of bounds"
                    , context, other.posStart, other.posEnd)
                );
            }
        }
        else
            return (null, IncompatibleTypeOperation(other));
    }

    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < elements.Count; i++)
        {
            s += elements[i].ToString() + (i != elements.Count - 1 ? ", " : "");
        }
        return string.Format("[{0}]", s);
    }
}
