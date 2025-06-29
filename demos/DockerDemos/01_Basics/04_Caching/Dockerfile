# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/sdk:9.0.300 AS build

# Expose port 8080 for the application
EXPOSE 8080

# Set the working directory in the container
WORKDIR /src

# Copy the source code of the repository
COPY . .

# Copy project files for different parts of the solution to enable caching of the restore step.
COPY ["FastFood.Common/FastFood.Common.csproj", "FastFood.Common/"]
COPY ["FinanceService/FinanceService.csproj", "FinanceService/"]
COPY ["FinanceService.Common/FinanceService.Common.csproj", "FinanceService.Common/"]

# Restore all the project files
RUN dotnet restore "FinanceService/FinanceService.csproj"

# Build the FinanceService project in Release mode and output the build results to /app/build.
RUN dotnet build "FinanceService/FinanceService.csproj" -c Release --no-restore
RUN dotnet publish "FinanceService/FinanceService.csproj" -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:9.0.3 AS base

WORKDIR /app

COPY --from=build /app/publish .

# Define the entry point for the container
ENTRYPOINT ["dotnet", "FinanceService.dll"]
