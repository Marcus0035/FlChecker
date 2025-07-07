namespace FlChecker2.Models;

public class PriceInfo
{
    public string Price { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
