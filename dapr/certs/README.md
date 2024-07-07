# Setup Certificates

Check the following [Link](https://docs.dapr.io/operations/security/mtls/#bringing-your-own-certificates-1) to get the tools and instructions.

This is for demo only!!! Do not store the certificates in the repository!

```bash
step certificate create cluster.local ca.crt ca.key --profile root-ca --no-password --insecure
step certificate create cluster.local issuer.crt issuer.key --ca ca.crt --ca-key ca.key --profile intermediate-ca --not-after 8760h --no-password --insecure
```