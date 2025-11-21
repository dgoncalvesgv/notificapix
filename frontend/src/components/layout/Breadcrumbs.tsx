import { Link, useLocation } from "react-router-dom";
import { useMemo } from "react";

type BreadcrumbItem = {
  label: string;
  to?: string;
};

const breadcrumbMap: Record<string, BreadcrumbItem[]> = {
  "/app/overview": [{ label: "Visão geral" }],
  "/app/transactions": [
    { label: "Visão geral", to: "/app/overview" },
    { label: "Transações PIX" }
  ],
  "/app/alerts": [
    { label: "Configurações", to: "/app/settings" },
    { label: "Alertas" }
  ],
  "/app/bank-connections": [
    { label: "Configurações", to: "/app/settings" },
    { label: "Conexões bancárias" }
  ],
  "/app/settings": [{ label: "Configurações" }],
  "/app/settings/profile": [
    { label: "Configurações", to: "/app/settings" },
    { label: "Dados do cadastro" }
  ],
  "/app/settings/security": [
    { label: "Configurações", to: "/app/settings" },
    { label: "Senha e segurança" }
  ],
  "/app/settings/notifications": [
    { label: "Configurações", to: "/app/settings" },
    { label: "Notificações" }
  ],
  "/app/settings/pix-keys": [
    { label: "Configurações", to: "/app/settings" },
    { label: "Chaves Pix" }
  ],
  "/app/team": [
    { label: "Configurações", to: "/app/settings" },
    { label: "Time" }
  ],
  "/app/billing": [
    { label: "Visão geral", to: "/app/overview" },
    { label: "Assinatura" }
  ],
  "/app/audit-logs": [
    { label: "Configurações", to: "/app/settings" },
    { label: "Audit Logs" }
  ],
  "/app/api-keys": [
    { label: "Configurações", to: "/app/settings" },
    { label: "API Keys" }
  ]
};

const buildFallback = (pathname: string): BreadcrumbItem[] => {
  const segments = pathname.split("/").filter(Boolean);
  if (!segments.length) {
    return [];
  }

  const items: BreadcrumbItem[] = [];
  let current = "";

  segments.forEach((segment, index) => {
    if (segment === "app") {
      items.push({ label: "Início", to: "/app/overview" });
      current = "/app";
      return;
    }

    current = `${current}/${segment}`;
    const label = segment
      .split("-")
      .map((token) => token.charAt(0).toUpperCase() + token.slice(1))
      .join(" ");

    items.push({
      label,
      to: index === segments.length - 1 ? undefined : current
    });
  });

  return items;
};

export const Breadcrumbs = () => {
  const location = useLocation();
  const items = useMemo(() => breadcrumbMap[location.pathname] ?? buildFallback(location.pathname), [location.pathname]);

  if (!items.length) {
    return null;
  }

  return (
    <nav className="text-xs md:text-sm text-slate-500 dark:text-slate-300 mb-4" aria-label="Breadcrumb">
      <ol className="flex flex-wrap items-center gap-1">
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          return (
            <li key={`${item.label}-${index}`} className="flex items-center gap-1">
              {item.to && !isLast ? (
                <Link to={item.to} className="hover:text-primary-600 dark:hover:text-primary-300 transition">
                  {item.label}
                </Link>
              ) : (
                <span className={isLast ? "text-slate-700 dark:text-white font-medium" : undefined}>{item.label}</span>
              )}
              {!isLast && <span className="text-slate-300 dark:text-slate-500">/</span>}
            </li>
          );
        })}
      </ol>
    </nav>
  );
};
