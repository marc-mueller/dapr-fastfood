# Step 1: Base image for the ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Step 2: Build stage for the .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0.100 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FrontendCustomerOrderStatus/FrontendCustomerOrderStatus.csproj", "FrontendCustomerOrderStatus/"]
COPY ["FastFood.Common/FastFood.Common.csproj", "FastFood.Common/"]
COPY ["OrderService.Common/OrderService.Common.csproj", "OrderService.Common/"]
RUN dotnet restore "FrontendCustomerOrderStatus/FrontendCustomerOrderStatus.csproj"
COPY . .
WORKDIR "/src/FrontendCustomerOrderStatus"
RUN dotnet build "FrontendCustomerOrderStatus.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Step 3: Publish stage for the .NET application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FrontendCustomerOrderStatus.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Step 4: Build stage for the client application
FROM node:18 AS client-build
WORKDIR /src
# Use separate COPY commands to ensure no issues with file existence
COPY FrontendCustomerOrderStatus/clientapp/package.json FrontendCustomerOrderStatus/clientapp/
COPY FrontendCustomerOrderStatus/clientapp/package-lock.json FrontendCustomerOrderStatus/clientapp/
WORKDIR /src/FrontendCustomerOrderStatus/clientapp
RUN npm install
COPY FrontendCustomerOrderStatus/clientapp .
RUN npm run build

# Step 5: Final stage to serve both the .NET and built client application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=client-build /src/FrontendCustomerOrderStatus/clientapp/dist ./wwwroot
ENTRYPOINT ["dotnet", "FrontendCustomerOrderStatus.dll"]