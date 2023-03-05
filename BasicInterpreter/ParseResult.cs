internal class ParseResult
{
    public int lastRegisteredAdvanceCount;
    public int advanceCount;
    public int toReverseCount;
    public Error error;
    public object node;

    public T Register<T>(ParseResult res)
    {
        lastRegisteredAdvanceCount = res.advanceCount;
        advanceCount += res.advanceCount;
        if (res.error != null) this.error = res.error;
        return (T)res.node;
    }

    public T TryRegister<T>(ParseResult res)
    {
        if (res.error != null)
        {
            toReverseCount = res.advanceCount;
            return default;
        }
        return Register<T>(res);
    }

    public void RegisterAdvancement()
    {
        lastRegisteredAdvanceCount = 1;
        advanceCount++;
    }

    public ParseResult Success<T>(T node)
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
