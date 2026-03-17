#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

write_step() {
  printf '\n==> %s\n' "$1"
}

write_note() {
  printf '  - %s\n' "$1"
}

write_success() {
  printf '  ✓ %s\n' "$1"
}

ensure_env_file() {
  if [[ ! -f "${REPO_ROOT}/.env.example" ]]; then
    echo "找不到 .env.example，請先確認 repo 是否完整。"
    exit 1
  fi

  if [[ ! -f "${REPO_ROOT}/.env" ]]; then
    cp "${REPO_ROOT}/.env.example" "${REPO_ROOT}/.env"
    write_success "已自動建立 .env，你可以之後再依環境需要調整。"
  fi
}

load_dotenv() {
  ensure_env_file
  set -a
  # shellcheck disable=SC1091
  source "${REPO_ROOT}/.env"
  set +a
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "找不到必要指令：$1"
    exit 1
  fi
}

port_is_available() {
  local port="$1"

  if command -v lsof >/dev/null 2>&1; then
    ! lsof -iTCP:"${port}" -sTCP:LISTEN -n -P >/dev/null 2>&1
    return
  fi

  if command -v ss >/dev/null 2>&1; then
    ! ss -ltn | awk '{print $4}' | grep -Eq "(^|:)${port}$"
    return
  fi

  return 0
}

docker_compose() {
  (
    cd "${REPO_ROOT}"
    docker compose --env-file .env "$@"
  )
}

dotnet_cmd() {
  (
    cd "${REPO_ROOT}"
    dotnet "$@"
  )
}
