# NotificaPix

Plataforma completa para monitoramento de créditos PIX multi-tenant com assinatura Stripe, notificações (e-mail / webhook), integrações Open Finance (mock) e dashboard React.

## Arquitetura

- **Backend**: ASP.NET Core 8 Minimal APIs, EF Core + Pomelo MySQL, JWT/RBAC multi-tenant, workers para polling PIX e disparo de alertas, Stripe SDK.
- **Frontend**: React + Vite + TypeScript + Tailwind, Zustand para sessão, React Router para dashboard organizado por áreas.
- **Infra**: Docker Compose (MySQL + API + Front), scripts utilitários, `.env.example`, seeds ricos.

```
         +-------------+         +----------------+        +-------------------+
         | OpenFinance |<------->| PixPollingJob  |------->| PixTransactions   |
         +-------------+         +----------------+        +-------------------+
                |                         |                        |
                v                         v                        v
        /bank/connect              /alerts/list           AlertDispatcherWorker
                \                         |                        |
                 +------------------------+------------------------+
                                      |
                              Notifications (Email/Webhook)
                                      |
                                      v
                                 Cliente Final
```

## Pré-requisitos

- .NET 8 SDK
- Node.js 20 + npm
- MySQL 8 (local ou via Docker)
- Conta Stripe (chaves test)

## Configuração rápida

```bash
cp .env.example .env
# ajuste credenciais / chaves Stripe
```

### Banco + migrations

```bash
cd backend
dotnet tool install --global dotnet-ef # se necessário
dotnet ef database update --project src/NotificaPix.Api/NotificaPix.Api.csproj --startup-project src/NotificaPix.Api/NotificaPix.Api.csproj
```

Seeds criam `admin@demo.com / P@ssword123`, org demo, 10 PIX, 5 alertas e conexão mock.

> **MySQL hospedado**: caso use o banco do host `193.203.175.133`, basta executar `mysql ... < backend/database/001_init.sql` conforme `backend/database/README.md`.

### Executar local

Backend:

```bash
cd backend
dotnet run --project src/NotificaPix.Api
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Docker (opcional):

```bash
cd deploy
docker-compose up --build
```

## Stripe

1. Crie produtos/preços e preencha `STRIPE_PRICE_*` no `.env`.
2. Configure webhook apontando para `https://<host>/webhooks/stripe` com segredo `STRIPE_WEBHOOK_SECRET`.
3. Use `/billing/checkout-session` e `/billing/portal` para testar contratação e portal.

## Fluxo E2E sugerido

1. Registrar usuário → cria Organização + OrgAdmin automaticamente.
2. `/billing/checkout-session` abre Stripe Checkout → assinatura.
3. Conectar banco (mock) em `/app/bank-connections`.
4. Worker `PixPollingWorker` insere PIX simulados.
5. `AlertDispatcherWorker` envia e-mail (fake) e webhook (mock) conforme NotificationSettings.
6. Dashboard mostra overview, transações, alertas, billing, time, audit logs e API keys.

## Critérios de pronto

- Login/registro funcionando com JWT persistido.
- Stripe Checkout/Portal mockando fallback offline.
- Conexão mock gera transações automaticamente.
- Alertas gerados e persistidos com HMAC.
- RBAC aplicado por role (`OrgAdmin` vs `OrgMember`).
- UI responsiva com estados de loading/empty.

## Scripts úteis

- `backend/scripts/generate-test-jwt.sh <userId> <orgId> <role>`
- `backend/scripts/generate-api-key.sh`

## Testes

```bash
cd backend/tests/NotificaPix.Tests
dotnet test
```

## Estrutura

```
backend/
  src/NotificaPix.Api        # Minimal APIs + endpoints
  src/NotificaPix.Core       # Entidades, DTOs, interfaces
  src/NotificaPix.Infrastructure # EF, serviços, workers, seeds
  tests/NotificaPix.Tests    # Exemplos de testes
frontend/
  src/pages/...              # Auth + dashboard
deploy/
  docker-compose.yml         # MySQL + backend + frontend
```
