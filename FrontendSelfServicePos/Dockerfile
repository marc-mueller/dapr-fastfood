FROM mcr.microsoft.com/dotnet/aspnet:9.0.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0.100 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FrontendSelfServicePos/FrontendSelfServicePos.csproj", "FrontendSelfServicePos/"]
RUN dotnet restore "FrontendSelfServicePos/FrontendSelfServicePos.csproj"
COPY . .
WORKDIR "/src/FrontendSelfServicePos"
RUN dotnet build "FrontendSelfServicePos.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FrontendSelfServicePos.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FrontendSelfServicePos.dll"]
