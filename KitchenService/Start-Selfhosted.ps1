dapr run `
    --app-id kitchenservice `
    --app-port 8701 `
    --dapr-http-port 3700 `
    --dapr-grpc-port 60700 `
    --config ../dapr/config/config.yaml `
    --resources-path ../dapr/components `
    dotnet run