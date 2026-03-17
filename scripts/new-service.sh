#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v pwsh >/dev/null 2>&1; then
  echo "scripts/new-service.sh 目前會呼叫 PowerShell 版本，請先安裝 pwsh 或直接使用 scripts/new-service.ps1。"
  exit 1
fi

pwsh -File "${SCRIPT_DIR}/new-service.ps1" "$@"
