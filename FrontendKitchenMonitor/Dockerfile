# Step 1: Base image for the ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Step 2: Build stage for the .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0.100 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FrontendKitchenMonitor/FrontendKitchenMonitor.csproj", "FrontendKitchenMonitor/"]
COPY ["FastFood.Common/FastFood.Common.csproj", "FastFood.Common/"]
COPY ["KitchenService.Common/KitchenService.Common.csproj", "KitchenService.Common/"]
RUN dotnet restore "FrontendKitchenMonitor/FrontendKitchenMonitor.csproj"
COPY . .
WORKDIR "/src/FrontendKitchenMonitor"
RUN dotnet build "FrontendKitchenMonitor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Step 3: Publish stage for the .NET application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FrontendKitchenMonitor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Step 4: Build stage for the client application
FROM node:18 AS client-build
WORKDIR /src
# Use separate COPY commands to ensure no issues with file existence
COPY FrontendKitchenMonitor/clientapp/package.json FrontendKitchenMonitor/clientapp/
COPY FrontendKitchenMonitor/clientapp/package-lock.json FrontendKitchenMonitor/clientapp/
WORKDIR /src/FrontendKitchenMonitor/clientapp
RUN npm install
COPY FrontendKitchenMonitor/clientapp .
RUN npm run build

# Step 5: Final stage to serve both the .NET and built client application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=client-build /src/FrontendKitchenMonitor/clientapp/dist ./wwwroot
ENTRYPOINT ["dotnet", "FrontendKitchenMonitor.dll"]