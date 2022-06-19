internal class RuntimeResult
{
    public Error error;
    public object value; // TODO runtime value?

    public object Register(object res)
    {
        if (res is RuntimeResult)
        {
            RuntimeResult r = (RuntimeResult)res;
            if (r.error != null) this.error = r.error;
            return r.value;
        }
        return res;
    }

    public RuntimeResult Success(object value)
    {
        this.value = value;
        return this;
    }

    public RuntimeResult Failure(Error error)
    {
        this.error = error;
        return this;
    }
}
