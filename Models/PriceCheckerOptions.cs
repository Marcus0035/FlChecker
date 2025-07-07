namespace FlChecker2.Models;

public class PriceCheckerOptions
{
    public const string SectionName = "PriceChecker";
    
    public string Url { get; set; } = string.Empty;
    public int CheckIntervalMinutes { get; set; } = 480; // 8 hours by default
    public string Selector { get; set; } = string.Empty;
    public int ElementIndex { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public int PageLoadTimeoutSeconds { get; set; } = 30;
    public int WaitForSelectorTimeoutSeconds { get; set; } = 10;
}
