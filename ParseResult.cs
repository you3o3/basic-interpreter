internal class ParseResult
{
    public int advanceCount;
    public Error error;
    public Node node;

    public object Register(object res)
    {
        ParseResult r = (ParseResult)res;
        advanceCount += r.advanceCount;
        if (r.error != null) this.error = r.error;
        return r.node;
    }

    public void RegisterAdvancement()
    {
        advanceCount++;
    }

    public ParseResult Success(Node node)
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
