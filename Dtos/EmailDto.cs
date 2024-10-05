namespace EmailProvider.Dtos;

public class EmailDto
{
    public string? Name { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Service { get; set; }
    public string? Message { get; set; }
}
