# FL Studio Price Checker

🎵 Automatic FL Studio price monitor th## 📧 Setting up Gmail Email Notifications

Email notifications are **already configured** with your credentials:
- **From:** marcus0035dev@gmail.com
- **To:** marek.dvoracek606@gmail.com  
- **App Password:** Configured ✅

The application will automatically send email notifications when the price drops below the previous price.

### How it works:
1. **Price monitoring** - checks FL Studio price every 5 minutes
2. **Price comparison** - compares current price with last known price
3. **Price drop detection** - if new price < old price
4. **Email sent** - beautiful HTML email with price comparison
5. **Console alert** - shows 📉 PRICE DROP DETECTED! message

### Email features:
- 🎨 **Beautiful HTML email** with price comparison
- 📊 **Old vs New price** clearly displayed
- 🛒 **Direct link** to FL Studio purchase page
- 📅 **Timestamp** of when price was checkedn the official Image-Line website and alerts on changes.

## 🚀 Features

- **Automatic price monitoring** - checks prices at regular intervals
- **JavaScript rendering** - uses Playwright for proper dynamic content loading
- **Configurable settings** - check interval, selector, timeout etc.
- **Email notifications** - automatically sends email when price drops 📧
- **Docker support** - easily deployable in containers
- **Logging** - detailed logs including price change alerts
- **Retry mechanism** - automatic retry on errors

## 📋 Requirements

- .NET 9.0
- Docker (optional)
- Internet connection
- Gmail account with App Password (for email notifications)

## ⚙️ Configuration

### First Time Setup

1. **Copy the example configuration:**
   ```bash
   copy appsettings.example.json appsettings.json
   ```

2. **Edit `appsettings.json` with your email credentials** (this file is gitignored for security)

Configuration is done through the `appsettings.json` file:

```json
{
  "PriceChecker": {
    "Url": "https://www.image-line.com/fl-studio/buy-now",
    "CheckIntervalMinutes": 480,
    "Selector": ".current-price",
    "ElementIndex": 2,
    "MaxRetries": 3,
    "PageLoadTimeoutSeconds": 30,
    "WaitForSelectorTimeoutSeconds": 10
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "your-email@gmail.com",
    "FromName": "FL Studio Price Checker",
    "ToEmail": "your-email@gmail.com",
    "Username": "your-email@gmail.com",
    "Password": "your-16-char-app-password"
  }
}
```

### Configuration parameters:

**Price Checker:**
- `Url` - URL of the page to monitor
- `CheckIntervalMinutes` - interval between checks (in minutes)
- `Selector` - CSS selector for price elements
- `ElementIndex` - element index (0-based), default is 2 for the third element
- `MaxRetries` - maximum number of retries on error
- `PageLoadTimeoutSeconds` - timeout for page loading
- `WaitForSelectorTimeoutSeconds` - timeout for waiting for selector

**Email Notifications:**
- `SmtpServer` - SMTP server (Gmail: smtp.gmail.com)
- `SmtpPort` - SMTP port (Gmail: 587)
- `FromEmail` - sender email address
- `FromName` - sender display name
- `ToEmail` - recipient email address
- `Username` - SMTP username (usually same as FromEmail)
- `Password` - Gmail App Password (16 characters)

## � Setting up Gmail Email Notifications

1. **Enable 2-Factor Authentication** on your Gmail account
2. **Generate App Password**:
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Generate a new app password (16 characters)
3. **Update appsettings.json** with your credentials:
   ```json
   "Email": {
     "FromEmail": "youremail@gmail.com",
     "ToEmail": "youremail@gmail.com", 
     "Username": "youremail@gmail.com",
     "Password": "your-16-char-app-password"
   }
   ```

The application will automatically send email notifications when the price drops below the previous price.

## 🐳 Running in Docker

### Quick start:
```bash
docker-compose up -d
```

### Manual build and run:
```bash
# Build image
docker build -t fl-checker .

# Run
docker run -d --name fl-checker \
  -v ./appsettings.json:/app/appsettings.json:ro \
  fl-checker
```

## 💻 Local execution

```bash
# Clone repository
git clone <your-repo-url>
cd FlChecker2

# Copy configuration
copy appsettings.example.json appsettings.json

# Edit appsettings.json with your Gmail credentials

# Restore packages
dotnet restore

# Install Playwright browsers
powershell bin/Debug/net9.0/playwright.ps1 install

# Run
dotnet run
```

## 🚀 GitHub Deployment

This project is GitHub-ready with proper security:

- ✅ **Sensitive data protected** - `appsettings.json` is gitignored
- ✅ **Example configuration** - `appsettings.example.json` included for setup
- ✅ **Docker ready** - can be deployed anywhere
- ✅ **No hardcoded secrets** - all credentials in config files

### Setup after cloning:

**Automatic setup:**
```bash
# Windows
setup.bat

# Linux/Mac
./setup.sh
```

**Manual setup:**
1. Copy `appsettings.example.json` to `appsettings.json`
2. Fill in your Gmail credentials
3. Run with Docker or dotnet

```bash
# Option 1: Docker
docker-compose up -d

# Option 2: Local .NET
dotnet run
```

## 📊 Output

The application logs the following information:

- ✅ **Unchanged price** - when price is same as last check
- 💰 **Price changed** - when price has changed
- � **Price dropped** - when price decreased (highlighted + email sent)
- 🏷️ **Initial price** - on first run
- ❌ **Errors** - when unable to fetch price

### Example output:
```
� [2025-01-07 10:30:00] FL Studio Price Check
   Status: DROPPED
   Price: €179.00
   🚨 PRICE DROP DETECTED! Email notification sent.
   URL: https://www.image-line.com/fl-studio/buy-now
   Checked at: 2025-01-07 09:30:00 UTC
--------------------------------------------------
```

## 🛠️ Technické detaily

### Architecture:
- **Hosted Service** - runs in background as Windows Service/Linux daemon
- **Playwright** - browser automation for JavaScript rendering
- **Email Notifications** - MailKit for Gmail SMTP
- **Dependency Injection** - modular architecture
- **Configuration** - flexible configuration via appsettings.json

### Technologies used:
- .NET 9.0
- Microsoft.Playwright
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Configuration
- MailKit (for email notifications)

## 📝 Logs

The application uses structured logging with different levels:

- `Information` - basic activity information
- `Warning` - price change alerts
- `Error` - scraping errors
- `Debug` - detailed information (debug mode only)

## 🔧 Troubleshooting

### Common issues:

1. **"Element not found"** - check `Selector` and `ElementIndex`
2. **"Page load timeout"** - increase `PageLoadTimeoutSeconds`
3. **"Browser failed to start"** - in Docker check Playwright dependencies
4. **"Email sending failed"** - verify Gmail App Password and settings

### Debug mode:
```bash
# Run with debug logs
dotnet run --environment Development
```

### Testing email functionality:
The email notifications will only trigger when a **price drop** is detected. To test:
1. Let the application run and establish a baseline price
2. If you want to force a test email, you can temporarily modify the price comparison logic
3. Check your email (marek.dvoracek606@gmail.com) for notifications

## 📄 License

MIT License - feel free to use and modify.

## 🤝 Contributing

1. Fork the project
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## ⚠️ Warning

This tool is intended for personal use only. When using, respect the Terms of Service of the target website and do not make too frequent requests.
