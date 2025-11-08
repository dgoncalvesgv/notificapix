#!/usr/bin/env bash
set -euo pipefail

USER_ID="${1:-00000000-0000-0000-0000-000000000001}"
ORG_ID="${2:-00000000-0000-0000-0000-000000000002}"
ROLE="${3:-OrgAdmin}"
SECRET="${JWT_SECRET:-supersecret}"

python3 - "$USER_ID" "$ORG_ID" "$ROLE" "$SECRET" <<'PY'
import base64, json, sys, hmac, hashlib, time
user_id, org_id, role, secret = sys.argv[1:5]
header = {"alg": "HS256", "typ": "JWT"}
payload = {
    "sub": user_id,
    "orgId": org_id,
    "role": role,
    "exp": int(time.time()) + 3600
}
def b64encode(data):
    return base64.urlsafe_b64encode(json.dumps(data).encode()).decode().rstrip("=")
segments = [b64encode(header), b64encode(payload)]
signature = hmac.new(secret.encode(), ".".join(segments).encode(), hashlib.sha256).digest()
segments.append(base64.urlsafe_b64encode(signature).decode().rstrip("="))
print(".".join(segments))
PY
