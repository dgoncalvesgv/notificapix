import { NavLink } from "react-router-dom";

const links = [
  { to: "/app/overview", label: "Overview" },
  { to: "/app/transactions", label: "Transações" },
  { to: "/app/alerts", label: "Alertas" },
  { to: "/app/bank-connections", label: "Conexões Bancárias", admin: true },
  { to: "/app/settings/notifications", label: "Notificações", admin: true },
  { to: "/app/team", label: "Time", admin: true },
  { to: "/app/billing", label: "Billing", admin: true },
  { to: "/app/audit-logs", label: "Audit Logs", admin: true },
  { to: "/app/api-keys", label: "API Keys", admin: true }
];

type SidebarProps = {
  role?: "OrgAdmin" | "OrgMember";
};

export const Sidebar = ({ role = "OrgMember" }: SidebarProps) => {
  return (
    <aside className="w-64 bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-slate-800 h-screen sticky top-0">
      <div className="px-6 py-4">
        <span className="text-lg font-semibold text-primary-600">NotificaPix</span>
      </div>
      <nav className="flex flex-col gap-1 px-2">
        {links
          .filter((link) => (link.admin ? role === "OrgAdmin" : true))
          .map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) =>
                `px-4 py-2 rounded-md text-sm font-medium ${
                  isActive ? "bg-primary-50 text-primary-600" : "text-slate-600 hover:bg-slate-100"
                }`
              }
            >
              {link.label}
            </NavLink>
          ))}
      </nav>
    </aside>
  );
};
