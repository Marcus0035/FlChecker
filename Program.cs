using FlChecker2.Models;
using FlChecker2.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Register configuration options
var priceCheckerOptions = new PriceCheckerOptions();
builder.Configuration.GetSection(PriceCheckerOptions.SectionName).Bind(priceCheckerOptions);
builder.Services.AddSingleton(priceCheckerOptions);

var emailOptions = new EmailOptions();
builder.Configuration.GetSection(EmailOptions.SectionName).Bind(emailOptions);
builder.Services.AddSingleton(emailOptions);

// Register services
builder.Services.AddSingleton<IPriceScrapingService, PriceScrapingService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<PriceMonitoringService>();

// Logging configuration
builder.Services.AddLogging(configure => configure.AddConsole());

var host = builder.Build();

Console.WriteLine("🚀 FL Studio Price Checker Started!");
Console.WriteLine($"📡 Monitoring: {priceCheckerOptions.Url}");
Console.WriteLine($"⏱️  Check interval: {priceCheckerOptions.CheckIntervalMinutes} minutes");
Console.WriteLine($"🎯 Target: {priceCheckerOptions.Selector} (element #{priceCheckerOptions.ElementIndex})");
Console.WriteLine($"🔄 Max retries: {priceCheckerOptions.MaxRetries}");
Console.WriteLine("📧 Email notifications: ENABLED (for price drops)");
Console.WriteLine($"✉️  Email recipient: {emailOptions.ToEmail}");
Console.WriteLine(new string('=', 60));

// Graceful shutdown
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    host.StopAsync();
};

await host.RunAsync();
