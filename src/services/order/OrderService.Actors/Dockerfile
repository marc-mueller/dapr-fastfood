# Define build arguments for the base ASP.NET and SDK images and their versions.
# These arguments allow us to easily change the base images if needed.
ARG IMAGE_NET_ASPNET=mcr.microsoft.com/dotnet/aspnet
ARG IMAGE_NET_SDK=mcr.microsoft.com/dotnet/sdk
ARG IMAGE_NET_ASPNET_VERSION=9.0.9
ARG IMAGE_NET_SDK_VERSION=9.0.305

# Allow a suffix to be appended to the final ASP.NET image for development customization.
# This can be overridden at build time with --build-arg ASPNET_SUFFIX=
ARG ASPNET_SUFFIX=-noble-chiseled-extra

# ---------------------------------------------------------------------
# Stage 1: Create the base runtime image from ASP.NET
# ---------------------------------------------------------------------

# Use the official ASP.NET runtime image with the specified version.
FROM ${IMAGE_NET_ASPNET}:${IMAGE_NET_ASPNET_VERSION} AS base
# Set the working directory in the container where the app will run.
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------------------------------------------------------------------
# Stage 2: Build the application
# ---------------------------------------------------------------------

# Use the .NET SDK image which includes tools for building the app.
FROM ${IMAGE_NET_SDK}:${IMAGE_NET_SDK_VERSION} AS build
# Set the working directory where the source code will be copied.
WORKDIR /src

# Copy project files for different parts of the solution to enable caching of the restore step.
COPY ["common/FastFood.Common/FastFood.Common.csproj", "common/FastFood.Common/"]
COPY ["common/FastFood.Observability.Common/FastFood.Observability.Common.csproj", "common/FastFood.Observability.Common/"]
COPY ["services/order/OrderService.Actors/OrderService.Actors.csproj", "services/order/OrderService.Actors/"]
COPY ["services/order/OrderService.Models/OrderService.Models.csproj", "services/order/OrderService.Models/"]
COPY ["services/finance/FinanceService.Common/FinanceService.Common.csproj", "services/finance/FinanceService.Common/"]
COPY ["services/order/OrderService.Common/OrderService.Common.csproj", "services/order/OrderService.Common/"]
COPY ["services/order/OrderService.Actors.Unit.Tests/OrderService.Actors.Unit.Tests.csproj", "services/order/OrderService.Actors.Unit.Tests/"]

# Run dotnet restore for each project.
# If a secret NuGet configuration file is provided, it will be used during restore.
RUN --mount=type=secret,id=nugetconfig,required=false \
    sh -c 'if [ -f /run/secrets/nugetconfig ]; then \
              dotnet restore --configfile /run/secrets/nugetconfig "services/order/OrderService.Actors/OrderService.Actors.csproj"; \
              dotnet restore --configfile /run/secrets/nugetconfig "services/order/OrderService.Actors.Unit.Tests/OrderService.Actors.Unit.Tests.csproj"; \
          else \
              dotnet restore "services/order/OrderService.Actors/OrderService.Actors.csproj"; \
              dotnet restore "services/order/OrderService.Actors.Unit.Tests/OrderService.Actors.Unit.Tests.csproj"; \
          fi'

# Copy the remainder of the source code to the container.
COPY . .

# Build the OrderService.Actors project in Release mode and output the build results to /app/build.
# The --no-restore flag is used because restore was already performed.
RUN dotnet build "services/order/OrderService.Actors/OrderService.Actors.csproj" -c Release -o /app/build --no-restore
RUN dotnet build "services/order/OrderService.Actors.Unit.Tests/OrderService.Actors.Unit.Tests.csproj" -c Release --no-restore

# ---------------------------------------------------------------------
# Stage 3: Run tests
# ---------------------------------------------------------------------

FROM build AS test
# Accept a build ID as an argument to label the test results; defaults to "localbuild".
ARG BUILDID=localbuild
# Label the container with the build ID for test results traceability.
LABEL testresults=${BUILDID}

# Execute tests for OrderService.Actors.Unit.Tests with code coverage.
RUN dotnet test --no-build -c Release --results-directory /testresults \
    --logger "trx;LogFileName=test_results_orderserviceactors.trx" \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=json%2cCobertura \
    /p:CoverletOutput=/testresults/coverage/ \
    -p:MergeWith=/testresults/coverage/coverage.json services/order/OrderService.Actors.Unit.Tests/OrderService.Actors.Unit.Tests.csproj

# ---------------------------------------------------------------------
# Stage 4: Publish the application
# ---------------------------------------------------------------------

FROM build AS publish
# Publish the OrderService.Actors project in Release mode to the /app/publish directory.
# The /p:UseAppHost=false flag prevents the generation of platform-specific executables.
RUN dotnet publish "services/order/OrderService.Actors/OrderService.Actors.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Modify file permissions: directories get 755 and files get 644 for better security and accessibility.
RUN find /app/publish -type d -exec chmod 755 {} \; && \
    find /app/publish -type f -exec chmod 644 {} \;

# ---------------------------------------------------------------------
# Stage 5: Create the final runtime image
# ---------------------------------------------------------------------

# Use the ASP.NET image with optional suffix applied.
FROM ${IMAGE_NET_ASPNET}:${IMAGE_NET_ASPNET_VERSION}${ASPNET_SUFFIX} AS final
# Set the working directory inside the container where the app will run.
WORKDIR /app

# Copy the published application from the publish stage into the final image.
COPY --from=publish /app/publish .

# Define the entry point of the container: run the OrderService.Actors.dll using dotnet.
ENTRYPOINT ["dotnet", "OrderService.Actors.dll"]
