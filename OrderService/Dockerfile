FROM mcr.microsoft.com/dotnet/aspnet:8.0.6 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0.302 AS build
WORKDIR /src
COPY ["OrderService/OrderService.csproj", "OrderService/"]
COPY ["FastFood.Common/FastFood.Common.csproj", "FastFood.Common/"]
COPY ["FinanceService.Common/FinanceService.Common.csproj", "FinanceService.Common/"]
COPY ["KitchenService.Common/KitchenService.Common.csproj", "KitchenService.Common/"]
COPY ["OrderService.Models/OrderService.Models.csproj", "OrderService.Models/"]
COPY ["OrderService.Common/OrderService.Common.csproj", "OrderService.Common/"]
RUN dotnet restore "OrderService/OrderService.csproj"
COPY . .
WORKDIR "/src/OrderService"
RUN dotnet build "OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]
