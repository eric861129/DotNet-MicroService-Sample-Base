#!/usr/bin/env bash
set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

require_azure=false
if [[ "${1:-}" == "--require-azure-cli" ]]; then
  require_azure=true
fi

write_step "檢查本機必要工具"

require_command dotnet
dotnet_version="$(dotnet --version)"
write_success ".NET SDK: ${dotnet_version}"

if [[ ! "${dotnet_version}" =~ ^9\. && ! "${dotnet_version}" =~ ^10\. ]]; then
  write_note "目前建議使用 .NET SDK 9 或 10，以免和 solution 目標框架不一致。"
fi

require_command docker
docker version >/dev/null
write_success "Docker 指令可正常使用"

docker compose version >/dev/null
write_success "docker compose 指令可正常使用"

if [[ "${require_azure}" == "true" ]]; then
  require_command az
  az bicep version >/dev/null
  write_success "Azure CLI / Bicep 指令可正常使用"
fi

ensure_env_file
load_dotenv

write_step "檢查常用埠是否可用"

ports=(
  "GATEWAY_PORT:${GATEWAY_PORT}"
  "AUTH_SERVICE_PORT:${AUTH_SERVICE_PORT}"
  "CATALOG_SERVICE_PORT:${CATALOG_SERVICE_PORT}"
  "INVENTORY_SERVICE_PORT:${INVENTORY_SERVICE_PORT}"
  "ORDERING_SERVICE_PORT:${ORDERING_SERVICE_PORT}"
  "NOTIFICATION_SERVICE_PORT:${NOTIFICATION_SERVICE_PORT}"
  "CATALOG_DB_PORT:${CATALOG_DB_PORT}"
  "ORDERING_DB_PORT:${ORDERING_DB_PORT}"
  "INVENTORY_DB_PORT:${INVENTORY_DB_PORT}"
  "NOTIFICATION_DB_PORT:${NOTIFICATION_DB_PORT}"
  "AUTH_DB_PORT:${AUTH_DB_PORT}"
  "RABBITMQ_AMQP_PORT:${RABBITMQ_AMQP_PORT}"
  "RABBITMQ_MANAGEMENT_PORT:${RABBITMQ_MANAGEMENT_PORT}"
  "OTEL_GRPC_PORT:${OTEL_GRPC_PORT}"
  "OTEL_HTTP_PORT:${OTEL_HTTP_PORT}"
  "PROMETHEUS_PORT:${PROMETHEUS_PORT}"
  "LOKI_PORT:${LOKI_PORT}"
  "TEMPO_PORT:${TEMPO_PORT}"
  "GRAFANA_PORT:${GRAFANA_PORT}"
)

busy=0
for item in "${ports[@]}"; do
  name="${item%%:*}"
  port="${item##*:}"

  if port_is_available "${port}"; then
    write_success "${name} 使用的埠 ${port} 可用"
  else
    echo "  ! ${name} 使用的埠 ${port} 已被占用"
    busy=1
  fi
done

if [[ "${busy}" -eq 1 ]]; then
  echo "請先釋放被占用的埠，或修改 .env 後重新執行。"
  exit 1
fi

write_step "前置檢查完成"
write_success "你現在可以執行 bash scripts/dev-up.sh 啟動環境"
