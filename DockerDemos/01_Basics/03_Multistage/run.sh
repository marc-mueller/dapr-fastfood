#!/bin/bash

# Define variables
IMAGE_NAME="financeservice-demo-multistage"

# Build the Docker image
docker build -t "$IMAGE_NAME" -f Dockerfile ../../../