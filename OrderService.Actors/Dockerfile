FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OrderService.Agents/OrderService.Agents.csproj", "OrderService.Agents/"]
RUN dotnet restore "OrderService.Agents/OrderService.Agents.csproj"
COPY . .
WORKDIR "/src/OrderService.Agents"
RUN dotnet build "OrderService.Agents.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.Agents.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.Agents.dll"]
