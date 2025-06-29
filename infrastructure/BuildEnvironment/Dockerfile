ARG NET_CORE_SDK_VERSION=9.0.300
ARG NODE_VERSION=22
ARG POWERSHELL_VERSION=7.4.10
ARG KUBECTL_VERSION=1.30.2
ARG HELM_VERSION=3.14.4

FROM mcr.microsoft.com/dotnet/sdk:${NET_CORE_SDK_VERSION}

ARG NET_CORE_SDK_VERSION
ARG NODE_VERSION
ARG POWERSHELL_VERSION
ARG SQLPACKAGE_URL=https://go.microsoft.com/fwlink/?linkid=2257477
ARG KUBECTL_VERSION
ARG HELM_VERSION

RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

RUN apt-get update && \
    apt-get install -y --no-install-recommends unzip wget ca-certificates curl gnupg jq && \
    rm -rf /var/lib/apt/lists/* && \
    wget -q -O /opt/sqlpackage.zip ${SQLPACKAGE_URL} && unzip -qq /opt/sqlpackage.zip -d /opt/sqlpackage && chmod +x /opt/sqlpackage/sqlpackage && rm -f /opt/sqlpackage.zip

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_${NODE_VERSION}.x | bash - && \
    apt-get install -y nodejs
RUN npm install rimraf -g
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