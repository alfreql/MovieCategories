namespace MovieCategories.Domain;

public class CustomException : Exception
{
    public int? Code { get; set; }
    public string? Details { get; set; }

    public CustomException(string? message, int? code = null, string? details = null) : base(message)
    {
        Code = code;
        Details = details;
    }
    public CustomException(string? message, Exception? innerException) : base(message, innerException) { }

    public override string ToString()
    {
        return base.ToString() + $". Code: {Code}, Details: {Details}";
    }
}