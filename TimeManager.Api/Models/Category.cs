namespace TimeManager.Api.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string ColorHex { get; set; } = null!;
}
