using FlChecker2.Models;
using FlChecker2.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FlChecker2.Services;

public class PriceMonitoringService : BackgroundService
{
    private readonly IPriceScrapingService _priceScrapingService;
    private readonly IEmailService _emailService;
    private readonly PriceCheckerOptions _options;
    private readonly ILogger<PriceMonitoringService> _logger;
    private PriceInfo? _lastPrice;

    public PriceMonitoringService(
        IPriceScrapingService priceScrapingService,
        IEmailService emailService,
        PriceCheckerOptions options,
        ILogger<PriceMonitoringService> logger)
    {
        _priceScrapingService = priceScrapingService;
        _emailService = emailService;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FL Studio Price Monitoring Service started");
        _logger.LogInformation("Monitoring URL: {Url}", _options.Url);
        _logger.LogInformation("Check interval: {IntervalMinutes} minutes", _options.CheckIntervalMinutes);
        _logger.LogInformation("Target selector: {Selector} (index: {Index})", _options.Selector, _options.ElementIndex);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPriceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in price monitoring loop");
            }

            // Wait before next check
            var delay = TimeSpan.FromMinutes(_options.CheckIntervalMinutes);
            _logger.LogDebug("Waiting {DelayMinutes} minutes until next check", delay.TotalMinutes);
            
            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("FL Studio Price Monitoring Service stopped");
    }

    private async Task CheckPriceAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting price check...");

        var retryCount = 0;
        PriceInfo? currentPrice = null;

        while (retryCount < _options.MaxRetries && currentPrice?.IsValid != true)
        {
            if (retryCount > 0)
            {
                _logger.LogWarning("Retrying price check (attempt {Attempt}/{MaxAttempts})", retryCount + 1, _options.MaxRetries);
                await Task.Delay(2000, cancellationToken); // 2 seconds between retries
            }

            currentPrice = await _priceScrapingService.GetCurrentPriceAsync(cancellationToken);
            retryCount++;
        }

        if (currentPrice?.IsValid == true)
        {
            await ProcessValidPrice(currentPrice);
        }
        else
        {
            _logger.LogError("Failed to get valid price after {MaxRetries} attempts. Last error: {Error}", 
                _options.MaxRetries, currentPrice?.ErrorMessage);
        }
    }

    private async Task ProcessValidPrice(PriceInfo currentPrice)
    {
        var hasChanged = _lastPrice == null || _lastPrice.Price != currentPrice.Price;
        var isPriceDropped = false;

        if (hasChanged && _lastPrice != null)
        {
            // Check if price has dropped
            isPriceDropped = IsPriceDropped(_lastPrice.Price, currentPrice.Price);
        }

        if (hasChanged)
        {
            if (_lastPrice == null)
            {
                _logger.LogInformation("🏷️  Initial price detected: {Price}", currentPrice.Price);
                
                // Send startup notification email
                try
                {
                    await _emailService.SendStartupNotificationAsync(currentPrice.Price, currentPrice.Url);
                    _logger.LogInformation("📧 Startup notification email sent successfully!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to send startup notification email");
                }
            }
            else
            {
                if (isPriceDropped)
                {
                    _logger.LogWarning("��💰 PRICE DROPPED! Old: {OldPrice} → New: {NewPrice}", 
                        _lastPrice.Price, currentPrice.Price);
                    
                    // Send email notification for price drop
                    try
                    {
                        await _emailService.SendPriceDropNotificationAsync(
                            _lastPrice.Price, 
                            currentPrice.Price, 
                            currentPrice.Url);
                        
                        _logger.LogInformation("📧 Price drop notification email sent successfully!");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to send price drop notification email");
                    }
                }
                else
                {
                    _logger.LogWarning("💰 PRICE CHANGED! Old: {OldPrice} → New: {NewPrice}", 
                        _lastPrice.Price, currentPrice.Price);
                }
            }
        }
        else
        {
            _logger.LogInformation("✅ Price unchanged: {Price}", currentPrice.Price);
        }

        _lastPrice = currentPrice;

        // Additional logic can be added here:
        // - Save to database
        // - Send other notifications
        // - Write to file
        await LogToConsole(currentPrice, hasChanged, isPriceDropped);
    }

    private bool IsPriceDropped(string oldPriceText, string newPriceText)
    {
        try
        {
            // Extract numeric values from price strings
            var oldValue = ExtractNumericValue(oldPriceText);
            var newValue = ExtractNumericValue(newPriceText);
            
            if (oldValue.HasValue && newValue.HasValue)
            {
                return newValue < oldValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to compare prices: {OldPrice} vs {NewPrice}", oldPriceText, newPriceText);
        }
        
        return false;
    }

    private decimal? ExtractNumericValue(string priceText)
    {
        if (string.IsNullOrWhiteSpace(priceText))
            return null;

        // Remove common currency symbols and whitespace
        var cleanText = priceText
            .Replace("Kč", "")
            .Replace("€", "")
            .Replace("$", "")
            .Replace("£", "")
            .Replace(".", "") // Remove thousands separator
            .Replace(",", ".") // Replace comma decimal separator with dot
            .Replace(" ", "")
            .Trim();

        if (decimal.TryParse(cleanText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }

    private async Task LogToConsole(PriceInfo priceInfo, bool hasChanged, bool isPriceDropped = false)
    {
        var status = hasChanged ? (isPriceDropped ? "DROPPED" : "CHANGED") : "SAME";
        var emoji = isPriceDropped ? "📉" : (hasChanged ? "🔄" : "📊");
        
        Console.WriteLine($"{emoji} [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FL Studio Price Check");
        Console.WriteLine($"   Status: {status}");
        Console.WriteLine($"   Price: {priceInfo.Price}");
        
        if (isPriceDropped)
        {
            Console.WriteLine($"   🚨 PRICE DROP DETECTED! Email notification sent.");
        }
        
        Console.WriteLine($"   URL: {priceInfo.Url}");
        Console.WriteLine($"   Checked at: {priceInfo.CheckedAt:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine(new string('-', 50));

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping FL Studio Price Monitoring Service...");
        
        if (_priceScrapingService is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
        
        await base.StopAsync(cancellationToken);
    }
}
