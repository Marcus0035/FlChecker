using FlChecker2.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace FlChecker2.Services;

public interface IEmailService
{
    Task SendPriceDropNotificationAsync(string oldPrice, string newPrice, string url, CancellationToken cancellationToken = default);
    Task SendStartupNotificationAsync(string currentPrice, string url, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailOptions options, ILogger<EmailService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendPriceDropNotificationAsync(string oldPrice, string newPrice, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(new MailboxAddress("", _options.ToEmail));
            message.Subject = "🚨 FL Studio Price Drop Alert!";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = CreateHtmlBody(oldPrice, newPrice, url),
                TextBody = CreateTextBody(oldPrice, newPrice, url)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            _logger.LogInformation("Connecting to SMTP server {Server}:{Port}", _options.SmtpServer, _options.SmtpPort);
            
            await client.ConnectAsync(_options.SmtpServer, _options.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            
            _logger.LogInformation("Sending price drop notification email to {ToEmail}", _options.ToEmail);
            
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            
            _logger.LogInformation("✅ Price drop notification email sent successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send price drop notification email");
        }
    }

    private string CreateHtmlBody(string oldPrice, string newPrice, string url)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #ff4444; color: white; padding: 20px; text-align: center; border-radius: 10px; }}
        .content {{ padding: 20px; border: 1px solid #ddd; border-radius: 10px; margin-top: 20px; }}
        .price-comparison {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .old-price {{ text-decoration: line-through; color: #888; }}
        .new-price {{ color: #00aa00; font-weight: bold; font-size: 1.2em; }}
        .button {{ background-color: #ff6600; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🚨 FL Studio Price Drop Alert!</h1>
    </div>
    
    <div class='content'>
        <h2>Great news! FL Studio price has dropped!</h2>
        
        <div class='price-comparison'>
            <p><strong>Previous price:</strong> <span class='old-price'>{oldPrice}</span></p>
            <p><strong>New price:</strong> <span class='new-price'>{newPrice}</span></p>
        </div>
        
        <p>The price monitoring detected a decrease in FL Studio pricing. This might be a good time to make your purchase!</p>
        
        <a href='{url}' class='button'>🛒 Check FL Studio Now</a>
        
        <hr style='margin-top: 30px;'>
        <p style='color: #666; font-size: 0.9em;'>
            This notification was sent by FL Studio Price Checker.<br>
            Checked at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
        </p>
    </div>
</body>
</html>";
    }

    private string CreateTextBody(string oldPrice, string newPrice, string url)
    {
        return $@"
🚨 FL Studio Price Drop Alert!

Great news! FL Studio price has dropped!

Previous price: {oldPrice}
New price: {newPrice}

The price monitoring detected a decrease in FL Studio pricing. 
This might be a good time to make your purchase!

Check FL Studio now: {url}

---
This notification was sent by FL Studio Price Checker.
Checked at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
";
    }

    public async Task SendStartupNotificationAsync(string currentPrice, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(new MailboxAddress("", _options.ToEmail));
            message.Subject = "📊 FL Studio Price Checker - Startup Notification";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = CreateStartupHtmlBody(currentPrice, url),
                TextBody = CreateStartupTextBody(currentPrice, url)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            _logger.LogInformation("Connecting to SMTP server {Server}:{Port}", _options.SmtpServer, _options.SmtpPort);
            
            await client.ConnectAsync(_options.SmtpServer, _options.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            
            _logger.LogInformation("Sending startup notification email to {ToEmail}", _options.ToEmail);
            
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            
            _logger.LogInformation("✅ Startup notification email sent successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send startup notification email");
        }
    }

    private string CreateStartupHtmlBody(string currentPrice, string url)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 10px; }}
        .content {{ padding: 20px; border: 1px solid #ddd; border-radius: 10px; margin-top: 20px; }}
        .current-price {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0; text-align: center; }}
        .price {{ color: #0066cc; font-weight: bold; font-size: 1.5em; }}
        .button {{ background-color: #ff6600; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>📊 FL Studio Price Checker Started</h1>
    </div>
    
    <div class='content'>
        <h2>Price monitoring has started successfully!</h2>
        
        <div class='current-price'>
            <p><strong>Current FL Studio price:</strong></p>
            <p class='price'>${currentPrice}</p>
        </div>
        
        <p>The FL Studio price checker is now running and will monitor for price changes. You will receive email notifications only when the price drops below the current level.</p>
        
        <a href='{url}' class='button'>🛒 Check FL Studio Now</a>
        
        <hr style='margin-top: 30px;'>
        <p style='color: #666; font-size: 0.9em;'>
            This notification was sent by FL Studio Price Checker.<br>
            Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
        </p>
    </div>
</body>
</html>";
    }

    private string CreateStartupTextBody(string currentPrice, string url)
    {
        return $@"
📊 FL Studio Price Checker Started

Price monitoring has started successfully!

Current FL Studio price: {currentPrice}

The FL Studio price checker is now running and will monitor for price changes. 
You will receive email notifications only when the price drops below the current level.

Check FL Studio now: {url}

---
This notification was sent by FL Studio Price Checker.
Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
";
    }
}
