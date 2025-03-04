# ----- Build Stage -----
FROM mcr.microsoft.com/dotnet/sdk:9.0.200 AS build
WORKDIR /src

# Install additional tools for AOT compilation
# RUN apt update
# RUN apt install -y clang zlib1g-dev

# Copy your project files
COPY . .

# Restore dependencies
RUN dotnet restore "FinanceService/FinanceService.csproj"

# Standard publish (non-optimized)
RUN dotnet publish "FinanceService/FinanceService.csproj" -c Release -o /app/publish

# Optimized publish with trimming
#RUN dotnet publish "FinanceService/FinanceService.csproj" -c Release -o /app/publish_optimized /p:PublishTrimmed=True /p:SelfContained=true

# Optimized publish with AOT
# RUN dotnet publish "FinanceService/FinanceService.csproj" -c Release -o /app/publish_aot -r linux-arm64 /p:PublishAot=True /p:StaticExecutable=True

RUN find /app/publish -type d -exec chmod 755 {} \; && \
    find /app/publish -type f -exec chmod 644 {} \;

# ----- Runtime Targets -----

# 1. Standard runtime image using the default ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0.2 AS base_standard
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinanceService.dll"]

# 2. Optimized build on the default ASP.NET runtime image
# FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.2 AS base_optimized
# WORKDIR /app
# COPY --from=build /app/publish_optimized .
# EXPOSE 8080
# ENTRYPOINT ["./FinanceService"]

# 3. Optimized build on the Alpine-based ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0.2-alpine3.21 AS base_alpine
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinanceService.dll"]

# 4. Optimized build on the chiseled ASP.NET runtime image (minimal footprint)
FROM mcr.microsoft.com/dotnet/aspnet:9.0.2-noble-chiseled AS base_chiseled
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinanceService.dll"]

# 5. Optimized build on the chiseled ASP.NET runtime image with icu or tzdata (minimal footprint)
FROM mcr.microsoft.com/dotnet/aspnet:9.0.2-noble-chiseled-extra AS base_chiseled_extra
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinanceService.dll"]

# 6. Optimized AOT build on apline
# FROM alpine:3.21 AS base_alpine_aot
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
# WORKDIR /app
# COPY --from=build /app/publish_aot/FinanceService .
# EXPOSE 8080
# ENTRYPOINT ["./FinanceService"]



