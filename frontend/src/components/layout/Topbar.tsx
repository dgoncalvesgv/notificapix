import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../../store/auth";

export const Topbar = () => {
  const { organization, user, clear } = useAuthStore();
  const navigate = useNavigate();
  const displayName = user?.name?.trim() || (user?.email ? user.email.split("@")[0] : "Usuário");
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const planDisplayName = organization?.planDisplayName ?? "Plano não definido";
  const hideTimeout = useRef<number>();
  const prefersDark = useMemo(
    () => (typeof window !== "undefined" && window.matchMedia?.("(prefers-color-scheme: dark)").matches) || false,
    []
  );
  const [theme, setTheme] = useState<"light" | "dark">(
    () => (localStorage.getItem("np_theme") as "light" | "dark") || (prefersDark ? "dark" : "light")
  );

  useEffect(() => {
    const root = document.documentElement;
    if (theme === "dark") {
      root.classList.add("dark");
    } else {
      root.classList.remove("dark");
    }
    localStorage.setItem("np_theme", theme);
  }, [theme]);

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
        <p className="text-xs uppercase tracking-wide text-slate-500 dark:text-slate-300">Organização</p>
        <p className="font-semibold text-slate-900 dark:text-white">{organization?.name ?? "—"}</p>
      </div>
      <div className="flex items-center gap-3">
        <button
          className="flex items-center gap-1 rounded-full border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-600 shadow-sm hover:bg-slate-50 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100"
          type="button"
          onClick={() => setTheme((prev) => (prev === "dark" ? "light" : "dark"))}
          aria-label="Alternar tema"
        >
          {theme === "dark" ? <SunIcon /> : <MoonIcon />}
        </button>
        <div className="relative" onMouseEnter={openMenu} onMouseLeave={scheduleClose}>
        <button
          className="flex items-center gap-2 rounded-full border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100"
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
          className={`absolute right-0 mt-2 w-48 rounded-xl border border-slate-200 bg-white shadow-lg transition z-50 dark:bg-slate-900 dark:border-slate-700 ${
            isMenuOpen ? "opacity-100 pointer-events-auto translate-y-0" : "opacity-0 pointer-events-none -translate-y-1"
          }`}
        >
          <div className="px-4 py-3 border-b border-slate-100 dark:border-slate-800">
            <p className="text-sm font-semibold text-slate-700 dark:text-white">{displayName}</p>
            <p className="text-xs text-slate-500 dark:text-slate-300">{organization?.name ?? "—"}</p>
          </div>
          <nav className="py-1">
            <button
              className="w-full text-left px-4 py-2 text-sm text-slate-600 hover:bg-primary-50/60 hover:text-primary-600 dark:text-slate-200 dark:hover:bg-primary-600/10 dark:hover:text-primary-300"
              onClick={() => navigate("/app/billing")}
            >
              <span className="block font-semibold text-slate-700 dark:text-white">Plano</span>
              <span className="text-xs text-slate-500 dark:text-slate-300">Atual: {planDisplayName}</span>
            </button>
            <button
              className="w-full text-left px-4 py-2 text-sm text-slate-600 hover:bg-primary-50/60 hover:text-primary-600 dark:text-slate-200 dark:hover:bg-primary-600/10 dark:hover:text-primary-300"
              onClick={() => navigate("/app/settings")}
            >
              Configurações
            </button>
            <button
              onClick={clear}
              className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 border-t border-slate-100 dark:border-slate-800 dark:hover:bg-red-500/10"
            >
              Sair
            </button>
          </nav>
        </div>
        </div>
      </div>
    </header>
  );
};

const SunIcon = () => (
  <svg className="h-4 w-4 text-amber-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="4" />
    <path d="M12 3v2M12 19v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M3 12h2M19 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42" />
  </svg>
);

const MoonIcon = () => (
  <svg className="h-4 w-4 text-slate-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
  </svg>
);
