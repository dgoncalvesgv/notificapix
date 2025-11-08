#!/usr/bin/env bash
set -euo pipefail

RAW_KEY=$(python3 - <<'PY'
import secrets
print(secrets.token_urlsafe(32))
PY
)

HASHED=$(python3 - <<'PY' "$RAW_KEY"
import sys, hashlib
print(hashlib.sha256(sys.argv[1].encode()).hexdigest().upper())
PY
)

echo "API Key (store securely): $RAW_KEY"
echo "Hash (store in DB): $HASHED"
