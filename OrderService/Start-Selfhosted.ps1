dapr run `
    --app-id orderservice `
    --app-port 8601 `
    --dapr-http-port 3600 `
    --dapr-grpc-port 60600 `
    --config ../dapr/config/config.yaml `
    --resources-path ../dapr/components `
    dotnet run