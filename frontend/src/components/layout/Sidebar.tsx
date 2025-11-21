import { useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuthStore } from "../../store/auth";

const links = [
  { to: "/app/overview", label: "Overview", icon: "dashboard" },
  { to: "/app/transactions", label: "Transações", icon: "transactions" },
  { to: "/app/pix/qr-codes", label: "Gerar QR Code de PIX", icon: "qr", admin: true }
];

type SidebarProps = {
  role?: "OrgAdmin" | "OrgMember";
};

export const Sidebar = ({ role = "OrgMember" }: SidebarProps) => {
  const [collapsed, setCollapsed] = useState(false);
  const organization = useAuthStore((state) => state.organization);
  const orgName = organization?.name ?? "NotificaPix";
  const shortName = orgName.split(" ")[0];
  return (
    <aside
      className={`${collapsed ? "w-20" : "w-64"} bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-slate-800 h-screen sticky top-0 transition-all`}
    >
      <div className="px-4 py-4 flex items-center justify-between">
        <span className="text-lg font-semibold text-primary-600">{collapsed ? shortName.slice(0, 2).toUpperCase() : shortName}</span>
        <button
          className="text-slate-500 hover:text-slate-700"
          onClick={() => setCollapsed((prev) => !prev)}
          aria-label="Colapsar menu"
        >
          {collapsed ? (
            <svg className="h-5 w-5" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 5L7 10L12 15" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          ) : (
            <svg className="h-5 w-5" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M8 5L13 10L8 15" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          )}
        </button>
      </div>
      <nav className="flex flex-col gap-1 px-2">
        {links
          .filter((link) => (link.admin ? role === "OrgAdmin" : true))
          .map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) =>
                `px-4 py-2 rounded-md text-sm font-medium flex items-center gap-3 transition ${
                  isActive
                    ? "bg-primary-50 text-primary-600"
                    : "text-slate-600 hover:bg-primary-50/40 hover:text-primary-600 dark:text-slate-300 dark:hover:bg-primary-600/10 dark:hover:text-primary-300"
                }`
              }
            >
              <Icon name={link.icon} />
              <span className={`${collapsed ? "hidden" : "block"}`}>{link.label}</span>
            </NavLink>
          ))}
      </nav>
    </aside>
  );
};

const Icon = ({ name }: { name: string }) => {
  const props = { className: "h-5 w-5", stroke: "currentColor", fill: "none", strokeWidth: 1.8, strokeLinecap: "round", strokeLinejoin: "round" } as const;
  switch (name) {
    case "dashboard":
      return (
        <svg viewBox="0 0 24 24" {...props}>
          <path d="M4 13h7V4H4zM13 4h7v5h-7zM13 11h7v9h-7zM4 16h7v4H4z" fill="currentColor" stroke="none" />
        </svg>
      );
    case "transactions":
      return (
        <svg viewBox="0 0 24 24" {...props}>
          <path d="M4 7h12M4 12h10M4 17h8M16 17l4-4-4-4" />
        </svg>
      );
    case "bank":
      return (
        <svg viewBox="0 0 24 24" {...props}>
          <path d="M3 9h18M5 9V6l7-3 7 3v3M5 20h14M7 20V9M12 20V9M17 20V9" />
        </svg>
      );
    case "team":
      return (
        <svg viewBox="0 0 24 24" {...props}>
          <path d="M7 14c-2.2 0-4 1.8-4 4v2h8v-2c0-2.2-1.8-4-4-4zM17 14c-2.2 0-4 1.8-4 4v2h8v-2c0-2.2-1.8-4-4-4z" />
          <circle cx="7" cy="8" r="3" stroke="currentColor" strokeWidth="1.8" />
          <circle cx="17" cy="8" r="3" stroke="currentColor" strokeWidth="1.8" />
        </svg>
      );
    case "billing":
      return (
        <svg viewBox="0 0 24 24" {...props}>
          <path d="M5 4h14v16H5zM5 9h14M9 4v5M9 13h2.5a2 2 0 110 4H9m0-4v4" />
        </svg>
      );
    case "qr":
      return (
        <svg viewBox="0 0 24 24" {...props}>
          <path d="M5 5h4v4H5zM15 5h4v4h-4zM5 15h4v4H5zM14 10h2v2h-2zM18 14h2v2h-2zM14 14h2v2h-2zM18 18h2v2h-2zM14 18h2v2h-2z" />
        </svg>
      );
    default:
      return (
        <svg viewBox="0 0 24 24" {...props}>
          <circle cx="12" cy="12" r="10" />
        </svg>
      );
  }
};
