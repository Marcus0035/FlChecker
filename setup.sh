#!/bin/bash

echo "🚀 FL Studio Price Checker - Initial Setup"
echo "=========================================="

if [ ! -f appsettings.json ]; then
    echo "✅ Copying example configuration..."
    cp appsettings.example.json appsettings.json
    echo
    echo "⚠️  IMPORTANT: Edit appsettings.json with your Gmail credentials!"
    echo "   - FromEmail: your-email@gmail.com"
    echo "   - ToEmail: your-email@gmail.com"
    echo "   - Username: your-email@gmail.com"
    echo "   - Password: your-16-char-app-password"
    echo
else
    echo "✅ appsettings.json already exists"
fi

if [ ! -f docker-compose.yml ]; then
    echo "✅ Copying example Docker Compose..."
    cp docker-compose.example.yml docker-compose.yml
    echo
else
    echo "✅ docker-compose.yml already exists"
fi

echo "📋 Next steps:"
echo "   1. Edit appsettings.json with your Gmail credentials"
echo "   2. Run: docker-compose up -d  (or dotnet run)"
echo
echo "🎯 The application will check FL Studio price every 8 hours"
echo "📧 You'll get email on startup and when price drops"
echo
