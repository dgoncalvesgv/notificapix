import { Link } from "react-router-dom";

const settingsLinks = [
  {
    title: "Chaves Pix",
    description: "Cadastre e gerencie suas chaves Pix para gerar QR codes.",
    to: "/app/settings/pix-keys"
  },
  {
    title: "Dados do cadastro",
    description: "Edite o nome da organização e o e-mail utilizado em cobranças.",
    to: "/app/settings/profile"
  },
  {
    title: "Senha e segurança",
    description: "Troque sua senha sempre que notar algo suspeito.",
    to: "/app/settings/security"
  },
  {
    title: "Alertas",
    description: "Configure regras e canais para disparo automático.",
    to: "/app/alerts"
  },
  {
    title: "Notificações",
    description: "Defina e-mails e webhooks para ser avisado.",
    to: "/app/settings/notifications"
  },
  {
    title: "Audit Logs",
    description: "Acompanhe todas as ações e mudanças na sua conta.",
    to: "/app/audit-logs"
  },
  {
    title: "API Keys",
    description: "Gerencie tokens de acesso para integrações externas.",
    to: "/app/api-keys"
  }
];

export const SettingsPage = () => {
  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold">Configurações</h1>
        <p className="text-sm text-slate-500">Gerencie preferências e integrações avançadas.</p>
      </header>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {settingsLinks.map((link) => (
          <Link
            key={link.to}
            to={link.to}
            className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm hover:border-primary-200 hover:shadow-md transition"
          >
            <h3 className="text-lg font-semibold text-slate-800">{link.title}</h3>
            <p className="text-sm text-slate-500 mt-1">{link.description}</p>
            <span className="text-sm font-medium text-primary-600 mt-4 inline-flex items-center gap-1">
              Acessar
              <svg
                className="h-4 w-4"
                viewBox="0 0 20 20"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  d="M7.5 5L12.5 10L7.5 15"
                  stroke="currentColor"
                  strokeWidth="1.6"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
            </span>
          </Link>
        ))}
      </div>
    </div>
  );
};
