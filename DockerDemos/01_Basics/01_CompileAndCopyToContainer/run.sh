#!/bin/bash

# Define variables
PROJECT_NAME="FinanceService"
PUBLISH_DIR="./publish"
IMAGE_NAME="financeservice-demo-compileandcopy"

# Clean up previous publish folder
if [ -d "$PUBLISH_DIR" ]; then
    rm -rf "$PUBLISH_DIR"
fi

# Publish the project
dotnet publish "../../../$PROJECT_NAME/$PROJECT_NAME.csproj" -c Release -o "$PUBLISH_DIR"

# Build the Docker image
docker build -t "$IMAGE_NAME" -f Dockerfile .