internal class ParseResult
{
    public int lastRegisteredAdvanceCount;
    public int advanceCount;
    public int toReverseCount;
    public Error error;
    public object node;

    public object Register(object res)
    {
        if (res is ParseResult r)
        {
            lastRegisteredAdvanceCount = advanceCount;
            advanceCount += r.advanceCount;
            if (r.error != null) this.error = r.error;
            return r.node;
        }
        return res;
    }

    public object TryRegister(object res)
    {
        if (res is ParseResult r)
        {
            if (r.error != null)
            {
                toReverseCount = advanceCount;
                return null;
            }
        }
        return Register(res);
    }

    public void RegisterAdvancement()
    {
        lastRegisteredAdvanceCount = 1;
        advanceCount++;
    }

    public ParseResult Success(object node)
    {
        this.node = node;
        return this;
    }

    public ParseResult Failure(Error error)
    {
        if (this.error == null || advanceCount == 0)
            this.error = error;
        return this;
    }
}
