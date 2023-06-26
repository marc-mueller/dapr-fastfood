dapr run `
    --app-id financeservice `
    --app-port 8901 `
    --dapr-http-port 3900 `
    --dapr-grpc-port 60900 `
    --config ../dapr/config/config.yaml `
    --resources-path ../dapr/components `
    dotnet run