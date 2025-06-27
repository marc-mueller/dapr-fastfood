#!/bin/bash
set -e

# Base image name prefix for the demo
IMAGE_NAME_BASE="financeservice-demo-imagesize"

# Build the Standard (non-optimized) image
docker build -t "${IMAGE_NAME_BASE}-standard" -f Dockerfile --target base_standard ../../

# Build the Optimized image (using default ASP.NET runtime)
#docker build -t "${IMAGE_NAME_BASE}-optimized" -f Dockerfile --target base_optimized ../../

# Build the Alpine optimized image
docker build -t "${IMAGE_NAME_BASE}-alpine" -f Dockerfile --target base_alpine ../../

# Build the Chiseled optimized image
docker build -t "${IMAGE_NAME_BASE}-chiseled" -f Dockerfile --target base_chiseled ../../

# Build the Chiseled extra optimized image
docker build -t "${IMAGE_NAME_BASE}-chiseled-extra" -f Dockerfile --target base_chiseled_extra ../../

# Build the AOT apline image
#docker build -t "${IMAGE_NAME_BASE}-alpine-aot" -f Dockerfile --target base_alpine_aot ../../

# List the built images with their sizes for comparison
echo "Docker images for comparison:"
docker images | grep "${IMAGE_NAME_BASE}"
