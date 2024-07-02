FROM mcr.microsoft.com/dotnet/aspnet:8.0.6 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0.302 AS build
WORKDIR /src
COPY ["FinanceService/FinanceService.csproj", "FinanceService/"]
COPY ["FinanceService.Common/FinanceService.Common.csproj", "FinanceService.Common/"]
RUN dotnet restore "FinanceService/FinanceService.csproj"
COPY . .
WORKDIR "/src/FinanceService"
RUN dotnet build "FinanceService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceService.dll"]
