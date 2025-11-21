import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../../store/auth";

export const Topbar = () => {
  const { organization, user, clear } = useAuthStore();
  const navigate = useNavigate();
  const displayName = user?.name?.trim() || (user?.email ? user.email.split("@")[0] : "Usuário");
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const hideTimeout = useRef<number>();

  const clearTimeoutRef = () => {
    if (hideTimeout.current) {
      window.clearTimeout(hideTimeout.current);
      hideTimeout.current = undefined;
    }
  };

  const openMenu = () => {
    clearTimeoutRef();
    setIsMenuOpen(true);
  };

  const scheduleClose = () => {
    clearTimeoutRef();
    hideTimeout.current = window.setTimeout(() => setIsMenuOpen(false), 500);
  };

  useEffect(
    () => () => {
      clearTimeoutRef();
    },
    []
  );

  return (
    <header className="flex items-center justify-between border-b border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 px-6 py-3">
      <div>
        <p className="text-xs uppercase tracking-wide text-slate-500">Organização</p>
        <p className="font-semibold">{organization?.name ?? "—"}</p>
      </div>
      <div className="relative" onMouseEnter={openMenu} onMouseLeave={scheduleClose}>
        <button
          className="flex items-center gap-2 rounded-full border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 shadow-sm"
          type="button"
          onClick={() => (isMenuOpen ? setIsMenuOpen(false) : openMenu())}
        >
          <span className="inline-flex h-8 w-8 items-center justify-center rounded-full bg-primary-100 text-primary-700">
            <svg
              className="h-5 w-5"
              viewBox="0 0 24 24"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <circle cx="12" cy="8" r="3.5" stroke="currentColor" strokeWidth="1.5" />
              <path
                d="M6 20v-1a6 6 0 016-6v0a6 6 0 016 6v1"
                stroke="currentColor"
                strokeWidth="1.5"
                strokeLinecap="round"
              />
            </svg>
          </span>
          <svg
            className="h-4 w-4 text-slate-500 transition group-hover:text-slate-700"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M5 7.5L10 12.5L15 7.5"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </button>
        <div
          className={`absolute right-0 mt-2 w-48 rounded-xl border border-slate-200 bg-white shadow-lg transition z-50 ${
            isMenuOpen ? "opacity-100 pointer-events-auto translate-y-0" : "opacity-0 pointer-events-none -translate-y-1"
          }`}
        >
          <div className="px-4 py-3 border-b border-slate-100">
            <p className="text-sm font-semibold text-slate-700">{displayName}</p>
            <p className="text-xs text-slate-500">{organization?.name ?? "—"}</p>
          </div>
          <nav className="py-1">
            <button
              className="w-full text-left px-4 py-2 text-sm text-slate-600 hover:bg-slate-50"
              onClick={() => navigate("/app/settings")}
            >
              Configurações
            </button>
            <button
              onClick={clear}
              className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-slate-50 border-t border-slate-100"
            >
              Sair
            </button>
          </nav>
        </div>
      </div>
    </header>
  );
};
