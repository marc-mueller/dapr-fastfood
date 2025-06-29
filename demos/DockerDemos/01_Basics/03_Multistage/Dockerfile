# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/sdk:9.0.300 AS build

# Expose port 8080 for the application
EXPOSE 8080

# Set the working directory in the container
WORKDIR /src

# Copy the source code of the repository
COPY . .

# Build the FinanceService project in Release mode and output the build results to /app/build.
RUN dotnet restore "FinanceService/FinanceService.csproj"
RUN dotnet build "FinanceService/FinanceService.csproj" -c Release
RUN dotnet publish "FinanceService/FinanceService.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0.3 AS base

WORKDIR /app

COPY --from=build /app/publish .

# Define the entry point for the container
ENTRYPOINT ["dotnet", "FinanceService.dll"]
