# Observability Demo Queries (aligned to your metrics)

This sheet uses the exact metric names seen in Grafana’s metric explorer. Histograms follow the Prometheus pattern: `_bucket`, `_count`, `_sum`. Counters end with `_total`. ASP.NET Core metrics include `_seconds` in the name.

Notes
- Your custom histograms with original names ending in `..._milliseconds` are exported as seconds. This shows up as names like `..._milliseconds_seconds_bucket`. Do NOT divide by 1000 in queries.
- Items histograms include the unit in the name (e.g., `..._item_count_items_*`).
- HTTP server metrics: `http_server_request_duration_seconds_*` (seconds).

## Business KPIs

1) Orders per minute (created / paid)
- Created: `sum(rate(orderservice_orders_created_total[5m])) * 60`
- Paid: `sum(rate(orderservice_orders_paid_total[5m])) * 60`

2) Conversion rate (%)
- `(sum(rate(orderservice_orders_paid_total[5m])) / sum(rate(orderservice_orders_created_total[5m]))) * 100`

3) Items per order (p50, p95)
- p50: `histogram_quantile(0.50, sum by (le) (rate(orderservice_orders_item_count_items_bucket[5m])))`
- p95: `histogram_quantile(0.95, sum by (le) (rate(orderservice_orders_item_count_items_bucket[5m])))`

4) Sales cycle (create → payment) duration
- p95 (seconds): `histogram_quantile(0.95, sum by (le) (rate(orderservice_orders_sales_duration_milliseconds_seconds_bucket[5m])))`
- avg (seconds): `sum(rate(orderservice_orders_sales_duration_milliseconds_seconds_sum[5m])) / sum(rate(orderservice_orders_sales_duration_milliseconds_seconds_count[5m]))`

5) Kitchen preparation durations (order-level)
- p95 (seconds): `histogram_quantile(0.95, sum by (le) (rate(kitchenservice_orders_preparation_duration_milliseconds_seconds_bucket[5m])))`

6) Kitchen item preparation durations
- p95 (seconds): `histogram_quantile(0.95, sum by (le) (rate(kitchenservice_orders_item_preparation_duration_milliseconds_seconds_bucket[5m])))`

## Service Performance (ASP.NET Core)

1) Request throughput (RPS)
- `sum(rate(http_server_request_duration_seconds_count[5m]))`

2) Latency SLOs (p50, p95)
- p50 (s): `histogram_quantile(0.50, sum by (le) (rate(http_server_request_duration_seconds_bucket[5m])))`
- p95 (s): `histogram_quantile(0.95, sum by (le) (rate(http_server_request_duration_seconds_bucket[5m])))`

3) Error rate (% of 5xx)
- `(sum(rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m])) / sum(rate(http_server_request_duration_seconds_count[5m]))) * 100`

4) In-flight requests
- `sum(http_server_active_requests)`

## Kitchen Service Metrics

1) Orders per minute
- `sum(rate(kitchenservice_orders_created_total[5m])) * 60`

2) Items per order (p95)
- `histogram_quantile(0.95, sum by (le) (rate(kitchenservice_orders_item_count_items_bucket[5m])))`

3) Prep durations (p50/p95)
- Order prep p50: `histogram_quantile(0.50, sum by (le) (rate(kitchenservice_orders_preparation_duration_milliseconds_seconds_bucket[5m])))`
- Order prep p95: `histogram_quantile(0.95, sum by (le) (rate(kitchenservice_orders_preparation_duration_milliseconds_seconds_bucket[5m])))`
- Item prep p95: `histogram_quantile(0.95, sum by (le) (rate(kitchenservice_orders_item_preparation_duration_milliseconds_seconds_bucket[5m])))`

## Logs (Loki / LogQL)

Your logs support pipeline field filters (after the pipe). Use `{label=...} | detected_level=...` form as you verified.

1) Error logs (last 5m) across all services
- `sum by (service_name) ( count_over_time({service_name=~".+"} | detected_level=~"(Error|Critical|Fatal)" [5m]) )`

2) Recent errors stream for a service
- `{service_name="OrderService"} | detected_level=~"(Error|Critical|Fatal)"`

3) Information logs (sanity test)
- `{service_name="OrderService"} | detected_level="Information"`

4) Error volume split by service (top 5)
- `topk(5, sum by (service_name) ( count_over_time({service_name=~".+"} | detected_level=~"(Error|Critical|Fatal)" [5m]) ))`

Tip: Use the derived TraceID (if present) to jump to Jaeger traces from logs.

## Traces (Jaeger)

- Use Grafana Explore with the Jaeger datasource. Filter by Service and Operation, then investigate long spans or error-tagged spans.

Troubleshooting
- If a query returns no data, use Explore to confirm label keys like `http_response_status_code`, `service`, or route labels. Adjust queries accordingly.
 - "No data" vs 0: Loki shows "No data" when nothing matches. To display zero in a Stat/Time series panel, you can use an expression like `sum by (service_name) ( count_over_time({...}[5m]) ) or vector(0)` or configure the panel to replace missing values with 0.
