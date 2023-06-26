dapr run `
    --app-id orderserviceactors `
    --app-port 8651 `
    --dapr-http-port 3650 `
    --dapr-grpc-port 60650 `
    --config ../dapr/config/config.yaml `
    --resources-path ../dapr/components `
    dotnet run