namespace H.LowCode.Application.Contracts;

public abstract class DataDtoBase
{
    public DataDtoBase()
    {
        CreatedTime = DateTime.Now;
    }

    public DateTime CreatedTime { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? ModifiedTime { get; set; }

    public string? ModifiedUser { get; set; }
}