{{/*
Expand the name of the chart.
*/}}
{{- define "financeservice.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "financeservice.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "financeservice.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "financeservice.labels" -}}
helm.sh/chart: {{ include "financeservice.chart" . }}
{{ include "financeservice.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "financeservice.selectorLabels" -}}
app.kubernetes.io/name: {{ include "financeservice.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the runtime service account to use
*/}}
{{- define "financeservice.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
  {{- if .Values.serviceAccount.name }}
    {{- .Values.serviceAccount.name }}
  {{- else }}
    {{- include "financeservice.fullname" . }}
  {{- end }}
{{- else }}
  {{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}


{{/*
Create the name of the deploy/migration service account to use
*/}}
{{- define "financeservice.deployServiceAccountName" -}}
{{- if .Values.deployServiceAccount.create }}
  {{- if .Values.deployServiceAccount.name }}
    {{- .Values.deployServiceAccount.name }}
  {{- else }}
    {{- printf "%s-deployment" (include "financeservice.fullname" .) }}
  {{- end }}
{{- else }}
  {{- default "default" .Values.deployServiceAccount.name }}
{{- end }}
{{- end }}
