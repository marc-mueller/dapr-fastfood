#!/bin/bash

# Define variables
IMAGE_NAME="financeservice-demo-buildandrun"

# Build the Docker image
docker build -t "$IMAGE_NAME" -f Dockerfile ../../../