#!/usr/bin/env bash
set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

remove_volumes=false
remove_orphans=false

for arg in "$@"; do
  case "${arg}" in
    --remove-volumes) remove_volumes=true ;;
    --remove-orphans) remove_orphans=true ;;
    *)
      echo "不支援的參數：${arg}"
      exit 1
      ;;
  esac
done

ensure_env_file

args=(down)
if [[ "${remove_volumes}" == "true" ]]; then
  args+=(--volumes)
fi

if [[ "${remove_orphans}" == "true" ]]; then
  args+=(--remove-orphans)
fi

write_step "停止並清除 docker compose 啟動的服務"
docker_compose "${args[@]}"
write_success "環境已停止"
