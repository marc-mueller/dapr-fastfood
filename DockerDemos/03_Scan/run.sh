#!/bin/bash
set -euo pipefail

# Define variables
IMAGE_NAME="financeservice-demo-scan"
VULN_OUTPUT="trivy_vuln_report.json"
SBOM_OUTPUT="trivy_sbom.spdx.json"

# Build the Docker image
echo "Building Docker image: $IMAGE_NAME"
docker build -t "$IMAGE_NAME" -f Dockerfile ../../

# --------------------------------------------
# Vulnerability Scan
# --------------------------------------------
# Best Practices:
# - Use severity filtering to focus on important issues (e.g. HIGH and CRITICAL)
# - Output the results in JSON format for automated analysis in CI/CD
echo "Scanning the image for vulnerabilities (HIGH and CRITICAL only)..."

# Show console output
trivy image "$IMAGE_NAME"

# Export the report
trivy image --severity HIGH,CRITICAL --format json -o "$VULN_OUTPUT" "$IMAGE_NAME"

echo "Vulnerability scan results saved to $VULN_OUTPUT"

# --------------------------------------------
# SBOM Extraction
# --------------------------------------------
# Trivy can generate an SBOM (Software Bill of Materials) that lists all components
# in your image. Here we extract the SBOM in SPDX format.
echo "Extracting SBOM in SPDX format..."
trivy image --format spdx-json -o "$SBOM_OUTPUT" "$IMAGE_NAME"
echo "SBOM saved to $SBOM_OUTPUT"

# --------------------------------------------
# Additional Examples (Optional)
# --------------------------------------------
# Example: Filesystem Scan
# You can scan a directory (e.g. application source code) for vulnerabilities.
# Uncomment the following lines if you wish to scan a filesystem directory.
# FS_SCAN_OUTPUT="trivy_fs_scan_report.json"
# echo "Scanning local filesystem directory /path/to/dir..."
# trivy fs --format json -o "$FS_SCAN_OUTPUT" /path/to/dir
# echo "Filesystem scan results saved to $FS_SCAN_OUTPUT"

# Example: Configuration Scan
# If you have configuration files (or want to scan your Dockerfile) for best practices,
# you can use Trivy's config scanning capabilities.
# CONFIG_SCAN_OUTPUT="trivy_config_report.json"
# echo "Scanning configuration files for misconfigurations..."
# trivy config --format json -o "$CONFIG_SCAN_OUTPUT" .
# echo "Configuration scan results saved to $CONFIG_SCAN_OUTPUT"

echo "All scans completed successfully."
