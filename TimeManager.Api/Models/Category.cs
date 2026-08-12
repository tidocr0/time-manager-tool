using System.ComponentModel.DataAnnotations;

namespace TimeManager.Api.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string ColorHex { get; set; } = null!;
}
