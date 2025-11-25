ARG IMAGE_NET_ASPNET=mcr.microsoft.com/dotnet/aspnet
ARG IMAGE_NET_SDK=mcr.microsoft.com/dotnet/sdk
ARG IMAGE_NET_ASPNET_VERSION=10.0.0
ARG IMAGE_NET_SDK_VERSION=10.0.100

FROM ${IMAGE_NET_ASPNET}:${IMAGE_NET_ASPNET_VERSION} AS base
WORKDIR /app

FROM ${IMAGE_NET_SDK}:${IMAGE_NET_SDK_VERSION} AS build
WORKDIR /src
COPY ["services/finance/FinanceService.Db/FinanceService.Db.sqlproj", "services/finance/FinanceService.Db/"]
COPY ["services/finance/FinanceService.Db.Cli/FinanceService.Db.Cli.csproj", "services/finance/FinanceService.Db.Cli/"]

RUN --mount=type=secret,id=nugetconfig,required=false \
    sh -c 'if [ -f /run/secrets/nugetconfig ]; then \
              dotnet restore --configfile /run/secrets/nugetconfig "services/finance/FinanceService.Db/FinanceService.Db.sqlproj"; \
              dotnet restore --configfile /run/secrets/nugetconfig "services/finance/FinanceService.Db.Cli/FinanceService.Db.Cli.csproj"; \
          else \
              dotnet restore "services/finance/FinanceService.Db/FinanceService.Db.sqlproj"; \
              dotnet restore "services/finance/FinanceService.Db.Cli/FinanceService.Db.Cli.csproj"; \
          fi' 

COPY ["services/finance/FinanceService.Db/", "services/finance/FinanceService.Db/"]
COPY ["services/finance/FinanceService.Db.Cli/", "services/finance/FinanceService.Db.Cli/"]

RUN dotnet build "services/finance/FinanceService.Db/FinanceService.Db.sqlproj" -c Release -o /app/dbschema --no-restore
RUN dotnet publish "services/finance/FinanceService.Db.Cli/FinanceService.Db.Cli.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
COPY --from=build /app/dbschema .

ENTRYPOINT ["dotnet", "FinanceService.Db.Cli.dll"]