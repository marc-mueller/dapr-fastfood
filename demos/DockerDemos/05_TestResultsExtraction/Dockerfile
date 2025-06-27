# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/sdk:9.0.200 AS build

# Expose port 8080 for the application
EXPOSE 8080

# Set the working directory in the container
WORKDIR /src

# Copy the source code of the repository
COPY . .

# Build the FinanceService project in Release mode and output the build results to /app/build.
RUN dotnet restore "FinanceService/FinanceService.csproj"
RUN dotnet restore "Tests/FinanceService.Unit.Tests/FinanceService.Unit.Tests.csproj"
RUN dotnet build "FinanceService/FinanceService.csproj" -c Release
RUN dotnet build "Tests/FinanceService.Unit.Tests/FinanceService.Unit.Tests.csproj" -c Release --no-restore
RUN dotnet publish "FinanceService/FinanceService.csproj" -c Release -o /app/publish

FROM build AS test
# Accept a build ID as an argument to label the test results; defaults to "localbuild".
ARG BUILDID=localbuild
# Label the container with the build ID for test results traceability.
LABEL testresults=${BUILDID}

# Execute tests for FinanceService.Unit.Tests with code coverage.
RUN dotnet test --no-build -c Release --results-directory /testresults \
    --logger "trx;LogFileName=test_results_financeservice.trx" \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=json%2cCobertura \
    /p:CoverletOutput=/testresults/coverage/ \
    -p:MergeWith=/testresults/coverage/coverage.json Tests/FinanceService.Unit.Tests/FinanceService.Unit.Tests.csproj

FROM mcr.microsoft.com/dotnet/aspnet:9.0.2 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Define the entry point for the container
ENTRYPOINT ["dotnet", "FinanceService.dll"]
