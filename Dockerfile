# Use the official .NET runtime as a base image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Install required dependencies for Playwright
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    ca-certificates \
    apt-transport-https \
    # Playwright dependencies
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    libgtk-3-0 \
    libasound2 \
    libatspi2.0-0 \
    libxss1 \
    libgconf-2-4 \
    libxfixes3 \
    libxrender1 \
    libxtst6 \
    libxi6 \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["FlChecker2.csproj", "."]
RUN dotnet restore "FlChecker2.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "FlChecker2.csproj" -c Release -o /app/build

# Install Playwright browsers in build stage
RUN dotnet /app/build/FlChecker2.dll --help || true
RUN pwsh /app/build/playwright.ps1 install chromium || true

# Publish the application
FROM build AS publish
RUN dotnet publish "FlChecker2.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy Playwright browsers from build stage
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

# Set environment variables
ENV PLAYWRIGHT_BROWSERS_PATH=/root/.cache/ms-playwright
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

ENTRYPOINT ["dotnet", "FlChecker2.dll"]
