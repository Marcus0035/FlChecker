using FlChecker2.Models;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace FlChecker2.Services;

public interface IPriceScrapingService
{
    Task<PriceInfo> GetCurrentPriceAsync(CancellationToken cancellationToken = default);
}

public class PriceScrapingService : IPriceScrapingService
{
    private readonly PriceCheckerOptions _options;
    private readonly ILogger<PriceScrapingService> _logger;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public PriceScrapingService(PriceCheckerOptions options, ILogger<PriceScrapingService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<PriceInfo> GetCurrentPriceAsync(CancellationToken cancellationToken = default)
    {
        var priceInfo = new PriceInfo
        {
            Url = _options.Url,
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            await InitializeBrowserAsync();
            
            if (_browser == null)
            {
                throw new InvalidOperationException("Browser not initialized");
            }

            var context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
            });

            var page = await context.NewPageAsync();
            
            _logger.LogInformation("Navigating to {Url}", _options.Url);
            
            await page.GotoAsync(_options.Url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = _options.PageLoadTimeoutSeconds * 1000
            });

            _logger.LogInformation("Waiting for selector {Selector}", _options.Selector);
            
            // Wait for price elements to load
            await page.WaitForSelectorAsync(_options.Selector, new PageWaitForSelectorOptions
            {
                Timeout = _options.WaitForSelectorTimeoutSeconds * 1000
            });

            // Get all elements with current-price class
            var priceElements = await page.QuerySelectorAllAsync(_options.Selector);
            
            if (priceElements.Count <= _options.ElementIndex)
            {
                throw new InvalidOperationException($"Element with index {_options.ElementIndex} not found. Found {priceElements.Count} elements.");
            }

            var targetElement = priceElements[_options.ElementIndex];
            var priceText = await targetElement.TextContentAsync();
            
            if (string.IsNullOrWhiteSpace(priceText))
            {
                throw new InvalidOperationException("Price text is empty");
            }

            priceInfo.Price = priceText.Trim();
            priceInfo.IsValid = true;
            
            _logger.LogInformation("Successfully extracted price: {Price}", priceInfo.Price);
            
            await context.CloseAsync();
            
            return priceInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while scraping price from {Url}", _options.Url);
            
            priceInfo.IsValid = false;
            priceInfo.ErrorMessage = ex.Message;
            
            return priceInfo;
        }
    }

    private async Task InitializeBrowserAsync()
    {
        if (_playwright == null)
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
            });
            
            _logger.LogInformation("Browser initialized");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        
        _playwright?.Dispose();
        
        _logger.LogInformation("Browser disposed");
    }
}
