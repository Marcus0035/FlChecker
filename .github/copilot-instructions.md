<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# FL Studio Price Checker - Copilot Instructions

This is a .NET 9.0 console application that monitors FL Studio prices on the official Image-Line website using web scraping with Playwright.

## Project Structure

- **Models**: Configuration and data models (`PriceCheckerOptions`, `PriceInfo`, `EmailOptions`)
- **Services**: Core business logic
  - `IPriceScrapingService`/`PriceScrapingService`: Web scraping using Playwright
  - `IEmailService`/`EmailService`: Email notifications using MailKit
  - `PriceMonitoringService`: Background service that orchestrates price checking
- **Program.cs**: Application entry point with dependency injection setup
- **Configuration**: appsettings.json for runtime configuration

## Key Technologies

- .NET 9.0 with Hosted Services
- Microsoft.Playwright for browser automation
- Microsoft.Extensions.Hosting for background services
- Microsoft.Extensions.Configuration for settings management
- MailKit for email notifications
- Docker support for containerization

## Architecture Patterns

- **Dependency Injection**: All services are registered in DI container
- **Background Service**: Uses `BackgroundService` for continuous monitoring
- **Configuration Pattern**: Strongly-typed configuration with `PriceCheckerOptions`
- **Async/Await**: All operations are asynchronous
- **Logging**: Structured logging throughout the application

## Important Implementation Details

- The application targets the 3rd element (index 2) with CSS selector `.current-price`
- Playwright runs in headless mode with specific browser arguments for Docker compatibility
- Retry mechanism handles temporary failures
- The service logs price changes prominently for monitoring
- **Email notifications**: Automatically sends email when price drops below previous price
- Price comparison uses numeric extraction to handle different currency formats (Kč, €, $)
- Graceful shutdown is implemented for proper resource cleanup

## Docker Configuration

- Multi-stage Dockerfile for optimized image size
- Playwright browsers are installed during build
- Required system dependencies for headless browser operation
- Docker Compose for easy deployment

## Development Guidelines

- Use async/await consistently
- Implement proper error handling with detailed logging
- Follow .NET naming conventions
- Use dependency injection for all services
- Maintain separation of concerns between scraping and monitoring logic
- Add comprehensive logging for debugging and monitoring
