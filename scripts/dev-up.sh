#!/usr/bin/env bash
set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

infrastructure_only=false
skip_prereqs=false
no_build=false

for arg in "$@"; do
  case "${arg}" in
    --infrastructure-only) infrastructure_only=true ;;
    --skip-prereqs) skip_prereqs=true ;;
    --no-build) no_build=true ;;
    *)
      echo "不支援的參數：${arg}"
      exit 1
      ;;
  esac
done

ensure_env_file
load_dotenv

if [[ "${skip_prereqs}" == "false" ]]; then
  bash "${SCRIPT_DIR}/check-prereqs.sh"
fi

infra_services=(
  catalog-db
  ordering-db
  inventory-db
  notification-db
  auth-db
  rabbitmq
  otel-collector
  prometheus
  loki
  tempo
  grafana
)

app_services=(
  auth-service-api
  catalog-service-api
  inventory-service-api
  ordering-service-api
  notification-service-api
  gateway-api
)

if [[ "${infrastructure_only}" == "true" ]]; then
  write_step "只啟動基礎設施容器"
  docker_compose up -d "${infra_services[@]}"
else
  write_step "啟動完整開發環境"
  if [[ "${no_build}" == "true" ]]; then
    docker_compose up -d "${infra_services[@]}" "${app_services[@]}"
  else
    docker_compose up --build -d "${infra_services[@]}" "${app_services[@]}"
  fi
fi

write_step "常用入口"
write_note "Gateway: http://localhost:${GATEWAY_PORT}"
write_note "Auth Service: http://localhost:${AUTH_SERVICE_PORT}"
write_note "Catalog Service: http://localhost:${CATALOG_SERVICE_PORT}"
write_note "Inventory Service: http://localhost:${INVENTORY_SERVICE_PORT}"
write_note "Ordering Service: http://localhost:${ORDERING_SERVICE_PORT}"
write_note "Notification Service: http://localhost:${NOTIFICATION_SERVICE_PORT}"
write_note "RabbitMQ 管理介面: http://localhost:${RABBITMQ_MANAGEMENT_PORT}"
write_note "Grafana: http://localhost:${GRAFANA_PORT}"
