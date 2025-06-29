# -------------------------------
# Stage 1: Build Stage
# -------------------------------
# Use the official .NET SDK image to build the application.
FROM mcr.microsoft.com/dotnet/sdk:9.0.300 AS build

# Set the working directory in the build container.
WORKDIR /src

# Copy all source code to the container.
COPY . .

# Restore NuGet packages for the FinanceService project.
RUN dotnet restore "FinanceService/FinanceService.csproj"

# Build the FinanceService project in Release mode.
RUN dotnet build "FinanceService/FinanceService.csproj" -c Release

# Publish the project and output the build artifacts to /app/publish.
RUN dotnet publish "FinanceService/FinanceService.csproj" -c Release -o /app/publish

# Modify file permissions: directories get 755 and files get 644 for better security and accessibility.
RUN find /app/publish -type d -exec chmod 755 {} \; && \
    find /app/publish -type f -exec chmod 644 {} \;


# -------------------------------
# Stage 2: Runtime (Final) Stage
# -------------------------------
# Use the official ASP.NET Core chiseled runtime image for the final container.
FROM mcr.microsoft.com/dotnet/aspnet:9.0.3-noble-chiseled AS runtime

# Set the working directory to /app.
WORKDIR /app

# Copy the published application from the build stage.
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FinanceService.dll"]
