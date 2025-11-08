## Provisionando o banco MySQL hospedado

Banco informado:

- Host/IP: `193.203.175.133`
- Base: `u360528542_notificapix`
- Usuário: `u360528542_notificapix`
- Senha: `X278g113`

### 1. Criar estrutura

```bash
mysql -h 193.203.175.133 \
      -u u360528542_notificapix \
      -pX278g113 \
      u360528542_notificapix \
      < backend/database/001_init.sql
```

O script replica exatamente o que as migrations do EF Core geram (tabelas, índices e chaves estrangeiras).

### 2. Configurar aplicação

No `.env` (ou `appsettings`), aponte para o mesmo host:

```
DB_HOST=193.203.175.133
DB_PORT=3306
DB_USER=u360528542_notificapix
DB_PASS=X278g113
DB_NAME=u360528542_notificapix
```

### 3. Seed inicial

Após a API subir com a conexão acima, o `DataSeeder` executa automaticamente (cria Org demo, admin etc). Se preferir controlar manualmente, desabilite o seeder e insira dados via scripts customizados.
