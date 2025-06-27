#!/usr/bin/env bash
set -euo pipefail

# 1) Namespace & SA
kubectl apply -f deployment-sa.yml

# 2) ClusterRole + Binding
kubectl apply -f deployment-manager-clusterrole.yml
kubectl apply -f deployment-manager-binding.yml

# 3) Remove any old token secret & recreate
kubectl delete secret deployment-sa-token -n cicd --ignore-not-found
kubectl apply -f deployment-sa-token-secret.yml

echo
echo "✅ ServiceAccount and RBAC are in place."
echo
echo "To fetch the token for your DevOps Kubernetes service connection, run:"
echo "  kubectl -n cicd get secret deployment-sa-token -o go-template='{{.data.token}}' | base64 --decode"
echo "and copy the resulting JWT."

echo "To fetch the secret value which is neeeded for the Azure DevOps Kubernetes service connection, run:"
echo "  kubectl get secret deployment-sa-token -n cicd -o json"
