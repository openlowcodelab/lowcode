namespace H.Util.Base;

public class BaseOutput
{
    public BaseOutput()
    {
        Success = true;
    }

    public BaseOutput(string message)
    {
        Success = false;
        Code = 1;
        Message = message;
    }

    public BaseOutput(int code, string message)
    {
        Code = code;
        Success = code == 0;
        Message = message;
    }

    public bool Success { get; set; }

    public int Code { get; set; }

    public string? Message { get; set; }
}

public class BaseOutput<T> : BaseOutput
{
    public T? Data { get; set; }

    public BaseOutput()
    {
    }

    public BaseOutput(T data)
    {
        Data = data;
        Success = true;
        Code = 0;
    }

    public BaseOutput(int code, string message)
    {
        Code = code;
        Success = code == 0;
        Message = message;
    }
}
