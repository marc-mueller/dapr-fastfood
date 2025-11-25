ARG NET_CORE_SDK_VERSION=10.0.100
ARG NODE_VERSION=24.1.0
ARG POWERSHELL_VERSION=7.4.10
ARG KUBECTL_VERSION=1.34.1
ARG HELM_VERSION=3.19.1

FROM mcr.microsoft.com/dotnet/sdk:${NET_CORE_SDK_VERSION}

ARG NET_CORE_SDK_VERSION
ARG NODE_VERSION
ARG POWERSHELL_VERSION
ARG SQLPACKAGE_URL=https://go.microsoft.com/fwlink/?linkid=2257477
ARG KUBECTL_VERSION
ARG HELM_VERSION
ARG TARGETARCH

RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

RUN apt-get update && \
    apt-get install -y --no-install-recommends unzip wget ca-certificates curl gnupg jq xz-utils && \
    rm -rf /var/lib/apt/lists/* && \
    wget -q -O /opt/sqlpackage.zip ${SQLPACKAGE_URL} && unzip -qq /opt/sqlpackage.zip -d /opt/sqlpackage && chmod +x /opt/sqlpackage/sqlpackage && rm -f /opt/sqlpackage.zip

# Install Node.js from official tarball
RUN set -eux; \
    arch="${TARGETARCH:-amd64}"; \
    case "$arch" in \
      amd64|x86_64) node_arch="x64" ;; \
      arm64|aarch64) node_arch="arm64" ;; \
      *) echo "Unsupported TARGETARCH: $arch"; exit 1 ;; \
    esac; \
    curl -fsSL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-${node_arch}.tar.xz" -o node.tar.xz; \
    mkdir -p /usr/local/lib/nodejs; \
    tar -xJf node.tar.xz -C /usr/local/lib/nodejs; \
    rm node.tar.xz; \
    ln -s /usr/local/lib/nodejs/node-v${NODE_VERSION}-linux-${node_arch}/bin/node /usr/local/bin/node; \
    ln -s /usr/local/lib/nodejs/node-v${NODE_VERSION}-linux-${node_arch}/bin/npm /usr/local/bin/npm; \
    ln -s /usr/local/lib/nodejs/node-v${NODE_VERSION}-linux-${node_arch}/bin/npx /usr/local/bin/npx
# Install global npm packages
RUN npm install -g rimraf
RUN npm install -g monotag

# Install PowerShell
RUN dotnet tool install --global powershell --version ${POWERSHELL_VERSION}
RUN pwsh -c "Install-Module SqlServer -Scope AllUsers -Force"

# Install kubectl
RUN curl -LO https://dl.k8s.io/release/v${KUBECTL_VERSION}/bin/linux/amd64/kubectl && \
    install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl && \
    rm kubectl

# Install Helm
RUN curl -LO https://get.helm.sh/helm-v${HELM_VERSION}-linux-amd64.tar.gz && \
    tar -zxvf helm-v${HELM_VERSION}-linux-amd64.tar.gz && \
    mv linux-amd64/helm /usr/local/bin/helm && \
    rm -rf linux-amd64 helm-v${HELM_VERSION}-linux-amd64.tar.gz

# Install Playwright via npm (aligns with project dependencies)
RUN npm install -g playwright@1.49.1

# Pre-install Playwright Chromium browser (most commonly used for headless testing)
# This ensures browsers are available in CI/CD without runtime downloads
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN npx playwright install chromium --with-deps

# Set environment variable for Playwright to skip browser download during restore
ENV PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1
