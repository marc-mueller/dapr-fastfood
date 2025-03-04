#!/bin/bash

# Define variables
IMAGE_NAME="financeservice-demo-secretsinbuild"
# Generate a GUID string for BUILDID (requires uuidgen to be installed)
buildid=$(uuidgen)

# Build the final Docker image
docker build -t "$IMAGE_NAME" -f Dockerfile --target final --secret id=nugetconfig,src=secretnugetcredentials.config ../../
