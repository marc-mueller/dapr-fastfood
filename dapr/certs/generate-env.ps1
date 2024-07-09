# Set the path to the certs directory
$certsPath = "$(Split-Path -Parent $MyInvocation.MyCommand.Path)"

# Read the certificate files
$trustAnchors = (Get-Content "$certsPath\ca.crt" -Raw) -replace "`r`n", "`n" -replace "`n", "\n"
$certChain = (Get-Content "$certsPath\issuer.crt" -Raw) -replace "`r`n", "`n" -replace "`n", "\n"
$certKey = (Get-Content "$certsPath\issuer.key" -Raw) -replace "`r`n", "`n" -replace "`n", "\n"

# Create the mtls.env file content
$envContent = @"
DAPR_TRUST_ANCHORS="$trustAnchors"
DAPR_CERT_CHAIN="$certChain"
DAPR_CERT_KEY="$certKey"
NAMESPACE=fastfood
"@

# Write the mtls.env file
$envContent | Out-File -FilePath "$certsPath\mtls.env" -Encoding utf8

Write-Host "Environment file generated successfully at $certsPath\mtls.env"
