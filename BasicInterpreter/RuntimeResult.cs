internal class RuntimeResult
{
    public Error error;
    public object value;
    public object funcReturnValue;

    public bool loopShouldContinue;
    public bool loopShouldBreak;

    public void Reset()
    {
        error = null;
        value = null;
        funcReturnValue = null;
        loopShouldContinue = false;
        loopShouldBreak = false;
    }

    public T Register<T>(RuntimeResult res)
    {
        if (res.error != null) error = res.error;
        funcReturnValue = res.funcReturnValue;
        loopShouldContinue = res.loopShouldContinue;
        loopShouldBreak = res.loopShouldBreak;
        return (T)res.value;
    }

    public RuntimeResult Success(object value)
    {
        Reset();
        this.value = value;
        return this;
    }

    public RuntimeResult SuccessReturn(object value)
    {
        Reset();
        funcReturnValue = value;
        return this;
    }

    public RuntimeResult SuccessContinue()
    {
        Reset();
        loopShouldContinue = true;
        return this;
    }

    public RuntimeResult SuccessBreak()
    {
        Reset();
        loopShouldBreak = true;
        return this;
    }

    public RuntimeResult Failure(Error error)
    {
        Reset();
        this.error = error;
        return this;
    }

    public bool ShouldReturn()
    {
        return (
            error != null ||
            funcReturnValue != null ||
            loopShouldContinue ||
            loopShouldBreak
        );
    }
}
