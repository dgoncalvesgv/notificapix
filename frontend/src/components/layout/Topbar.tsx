import { useAuthStore } from "../../store/auth";

export const Topbar = () => {
  const { organization, user, clear } = useAuthStore();
  return (
    <header className="flex items-center justify-between border-b border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 px-6 py-3">
      <div>
        <p className="text-xs uppercase tracking-wide text-slate-500">Organização</p>
        <p className="font-semibold">{organization?.name ?? "—"}</p>
      </div>
      <div className="flex items-center gap-4">
            <span className="text-sm text-slate-600">{user?.email}</span>
            <button
              onClick={clear}
              className="text-xs uppercase tracking-wide text-primary-600 border border-primary-100 px-3 py-1 rounded"
            >
              Sair
            </button>
      </div>
    </header>
  );
};
