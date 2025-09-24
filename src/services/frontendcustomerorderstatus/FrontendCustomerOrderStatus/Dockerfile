# Define build arguments for the base ASP.NET and SDK images and their versions.
# These arguments allow us to easily change the base images if needed.
ARG IMAGE_NET_ASPNET=mcr.microsoft.com/dotnet/aspnet
ARG IMAGE_NET_SDK=mcr.microsoft.com/dotnet/sdk
ARG IMAGE_NET_ASPNET_VERSION=9.0.9
ARG IMAGE_NET_SDK_VERSION=9.0.305

# Allow a suffix to be appended to the final ASP.NET image for development customization.
# This can be overridden at build time with --build-arg ASPNET_SUFFIX=-noble-chiseled-extra
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
COPY ["services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/FrontendCustomerOrderStatus.csproj", "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/"]
COPY ["common/FastFood.Common/FastFood.Common.csproj", "common/FastFood.Common/"]
COPY ["common/FastFood.Observability.Common/FastFood.Observability.Common.csproj", "common/FastFood.Observability.Common/"]
COPY ["services/order/OrderService.Common/OrderService.Common.csproj", "services/order/OrderService.Common/"]
COPY ["services/frontendcustomerorderstatus/FrontendCustomerOrderStatus.Unit.Tests/FrontendCustomerOrderStatus.Unit.Tests.csproj", "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus.Unit.Tests/"]

# Run dotnet restore for each project.
# If a secret NuGet configuration file is provided, it will be used during restore.
RUN --mount=type=secret,id=nugetconfig,required=false \
    sh -c 'if [ -f /run/secrets/nugetconfig ]; then \
              dotnet restore --configfile /run/secrets/nugetconfig "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/FrontendCustomerOrderStatus.csproj"; \
              dotnet restore --configfile /run/secrets/nugetconfig "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus.Unit.Tests/FrontendCustomerOrderStatus.Unit.Tests.csproj"; \
          else \
              dotnet restore "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/FrontendCustomerOrderStatus.csproj"; \
              dotnet restore "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus.Unit.Tests/FrontendCustomerOrderStatus.Unit.Tests.csproj"; \
          fi'

# Copy the remainder of the source code to the container.
COPY . .

# Build the FrontendCustomerOrderStatus project in Release mode and output the build results to /app/build.
# The --no-restore flag is used because restore was already performed.
RUN dotnet build "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/FrontendCustomerOrderStatus.csproj" -c Release -o /app/build --no-restore
RUN dotnet build "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus.Unit.Tests/FrontendCustomerOrderStatus.Unit.Tests.csproj" -c Release --no-restore

# ---------------------------------------------------------------------
# Stage 3: Run tests
# ---------------------------------------------------------------------

FROM build AS test
# Accept a build ID as an argument to label the test results; defaults to "localbuild".
ARG BUILDID=localbuild
# Label the container with the build ID for test results traceability.
LABEL testresults=${BUILDID}

# Execute tests for FrontendCustomerOrderStatus.Unit.Tests with code coverage.
RUN dotnet test --no-build -c Release --results-directory /testresults \
    --logger "trx;LogFileName=test_results_frontendcustomerorderstatus.trx" \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=json%2cCobertura \
    /p:CoverletOutput=/testresults/coverage/ \
    -p:MergeWith=/testresults/coverage/coverage.json services/frontendcustomerorderstatus/FrontendCustomerOrderStatus.Unit.Tests/FrontendCustomerOrderStatus.Unit.Tests.csproj

# ---------------------------------------------------------------------
# Stage 4: Publish the application
# ---------------------------------------------------------------------

FROM build AS publish
# Publish the FrontendCustomerOrderStatus project in Release mode to the /app/publish directory.
# The /p:UseAppHost=false flag prevents the generation of platform-specific executables.
RUN dotnet publish "services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/FrontendCustomerOrderStatus.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Modify file permissions: directories get 755 and files get 644 for better security and accessibility.
RUN find /app/publish -type d -exec chmod 755 {} \; && \
    find /app/publish -type f -exec chmod 644 {} \;

# ---------------------------------------------------------------------
# Stage 5: Build the client application
# ---------------------------------------------------------------------

FROM node:18 AS client-build
WORKDIR /src
# Use separate COPY commands to ensure no issues with file existence
COPY services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/clientapp/package.json services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/clientapp/
COPY services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/clientapp/package-lock.json services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/clientapp/
WORKDIR /src/services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/clientapp
# If a secret npmrc configuration file is provided, it will be used during npm install.
RUN --mount=type=secret,id=npmrc,required=false \
    sh -c 'if [ -f /run/secrets/npmrc ]; then \
              npm install --userconfig /run/secrets/npmrc; \
          else \
              npm install; \
          fi'
COPY services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/clientapp .
RUN npm run build


# ---------------------------------------------------------------------
# Stage 6: Create the final runtime image to serve both the .NET and built client application
# ---------------------------------------------------------------------

# Use the ASP.NET image with optional suffix applied.
FROM ${IMAGE_NET_ASPNET}:${IMAGE_NET_ASPNET_VERSION}${ASPNET_SUFFIX} AS final
# Set the working directory inside the container where the app will run.
WORKDIR /app

# Copy the published application from the publish stage into the final image.
COPY --from=publish /app/publish .
COPY --from=client-build /src/services/frontendcustomerorderstatus/FrontendCustomerOrderStatus/clientapp/dist ./wwwroot

# Define the entry point of the container: run the FrontendCustomerOrderStatus.dll using dotnet.
ENTRYPOINT ["dotnet", "FrontendCustomerOrderStatus.dll"]