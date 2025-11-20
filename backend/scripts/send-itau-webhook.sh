#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 2 ]]; then
  echo "Uso: $0 <api_key> <id_conta> [url]" >&2
  echo "Exemplo: $0 12345678-aaaa-bbbb-cccc-123456789abc 6070119011110000000012345 http://localhost:5089/integrations/itau/webhook" >&2
  exit 1
fi

API_KEY="$1"
ACCOUNT_ID="$2"
WEBHOOK_URL="${3:-http://localhost:5089/integrations/itau/webhook}"

PAYLOAD_TEMPLATE="$(cat <<'JSON'
{
  "id_evento": "evt-itau-demo-001",
  "id_conta": "__ACCOUNT__",
  "lancamentos": [
    {
      "id_lancamento": "b1ff5cc0-8a9c-497e-b983-738904c23389",
      "literal_lancamento": "PIX QRS PMD Demo 12/06",
      "tipo_lancamento": "pagamento",
      "tipo_operacao": "credito",
      "tipo_pix": "dinamico",
      "detalhe_pagamento": {
        "id_pagamento": "E60701190202205301441DY50YKTJG8A",
        "id_transferencia": "TRX123456789",
        "txid": "c3df5cc0-2a0c-381e-b207-738904c23555",
        "valor": 125.40,
        "data": "2024-06-07T10:32:00Z",
        "texto_resposta_pagador": "Pedido #NPX-1021",
        "debitado": {
          "nome": "Pagador Demo",
          "numero_documento": "65481904594",
          "conta": "0000000675432",
          "agencia": "3200"
        },
        "creditado": {
          "nome": "Loja Notifica",
          "numero_documento": "12345678000199",
          "conta": "0000000123456",
          "agencia": "1111",
          "chave_enderecamento": "pix@notifica.com"
        }
      }
    }
  ]
}
JSON
)"

PAYLOAD="${PAYLOAD_TEMPLATE/__ACCOUNT__/$ACCOUNT_ID}"
SIGNATURE="$(printf '%s' "$PAYLOAD" | openssl dgst -sha256 -hmac "$API_KEY" -binary | base64)"

echo "Enviando webhook para $WEBHOOK_URL"
curl -sS -X POST \
  -H "Content-Type: application/json" \
  -H "X-Itau-Event: lancamentos_pix" \
  -H "X-Itau-Signature: $SIGNATURE" \
  -d "$PAYLOAD" \
  "$WEBHOOK_URL"

echo
echo "Webhook enviado com sucesso."
