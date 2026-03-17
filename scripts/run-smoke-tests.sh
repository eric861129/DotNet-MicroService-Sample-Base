#!/usr/bin/env bash
set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

with_container_tests=false
against_running_stack=false

for arg in "$@"; do
  case "${arg}" in
    --with-container-tests) with_container_tests=true ;;
    --against-running-stack) against_running_stack=true ;;
    *)
      echo "不支援的參數：${arg}"
      exit 1
      ;;
  esac
done

bash "${SCRIPT_DIR}/check-prereqs.sh"
load_dotenv

write_step "執行 restore / build / test"
dotnet_cmd restore EnterpriseMicroservicesBoilerplate.sln
dotnet_cmd build EnterpriseMicroservicesBoilerplate.sln -c Debug --no-restore

if [[ "${with_container_tests}" == "true" ]]; then
  write_note "已啟用 Testcontainers smoke test"
  RUN_CONTAINER_TESTS=true dotnet_cmd test EnterpriseMicroservicesBoilerplate.sln -c Debug --no-build
else
  dotnet_cmd test EnterpriseMicroservicesBoilerplate.sln -c Debug --no-build
fi

if [[ "${against_running_stack}" == "true" ]]; then
  write_step "檢查執行中的服務健康狀態"

  curl --fail --silent "http://localhost:${GATEWAY_PORT}/" >/dev/null
  write_success "Gateway 回應正常"

  curl --fail --silent "http://localhost:${AUTH_SERVICE_PORT}/connect/health" >/dev/null
  write_success "AuthService 回應正常"

  curl --fail --silent "http://localhost:${CATALOG_SERVICE_PORT}/health" >/dev/null
  write_success "CatalogService 回應正常"

  curl --fail --silent "http://localhost:${INVENTORY_SERVICE_PORT}/health" >/dev/null
  write_success "InventoryService 回應正常"

  curl --fail --silent "http://localhost:${ORDERING_SERVICE_PORT}/health" >/dev/null
  write_success "OrderingService 回應正常"

  curl --fail --silent "http://localhost:${NOTIFICATION_SERVICE_PORT}/health" >/dev/null
  write_success "NotificationService 回應正常"
fi
