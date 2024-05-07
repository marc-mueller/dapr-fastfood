FROM mcr.microsoft.com/dotnet/aspnet:8.0.4 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0.204 AS build
WORKDIR /src
COPY ["OrderService.Actors/OrderService.Actors.csproj", "OrderService.Actors/"]
COPY ["FastFood.Common/FastFood.Common.csproj", "FastFood.Common/"]
COPY ["OrderService.Models/OrderService.Models.csproj", "OrderService.Models/"]
COPY ["FinanceService.Common/FinanceService.Common.csproj", "FinanceService.Common/"]
COPY ["OrderService.Common/OrderService.Common.csproj", "OrderService.Common/"]
RUN dotnet restore "OrderService.Actors/OrderService.Actors.csproj"
COPY . .
WORKDIR "/src/OrderService.Actors"
RUN dotnet build "OrderService.Actors.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.Actors.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.Actors.dll"]
