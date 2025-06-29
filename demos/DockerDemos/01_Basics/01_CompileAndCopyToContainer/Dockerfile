# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0.3 AS base

# Set the working directory in the container
WORKDIR /app

# Copy the published output from the host to the container
COPY ./publish .

# Expose port 8080 for the application
EXPOSE 8080

# Define the entry point for the container
ENTRYPOINT ["dotnet", "FinanceService.dll"]
