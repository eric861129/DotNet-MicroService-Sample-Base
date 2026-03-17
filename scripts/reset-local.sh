#!/usr/bin/env bash
set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

keep_env_file=false

for arg in "$@"; do
  case "${arg}" in
    --keep-env-file) keep_env_file=true ;;
    *)
      echo "不支援的參數：${arg}"
      exit 1
      ;;
  esac
done

write_step "停止容器並刪除 volumes"
bash "${SCRIPT_DIR}/dev-down.sh" --remove-volumes --remove-orphans

write_step "清除本機建置暫存"
find "${REPO_ROOT}" -type d \( -name bin -o -name obj -o -name TestResults -o -name .vs \) -prune -exec rm -rf {} +
write_success "已清除 bin / obj / TestResults / .vs"

if [[ "${keep_env_file}" == "false" && -f "${REPO_ROOT}/.env" ]]; then
  rm -f "${REPO_ROOT}/.env"
  write_success "已刪除 .env，下次執行會從 .env.example 重新建立"
fi
