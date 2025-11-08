import { Outlet, Link } from "react-router-dom";

export const AuthShell = () => (
  <div className="min-h-screen flex flex-col items-center justify-center bg-slate-50 dark:bg-slate-900 px-4">
    <Link to="/" className="text-2xl font-bold text-primary-600 mb-6">
      NotificaPix
    </Link>
    <div className="w-full max-w-md bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl p-6 shadow-lg">
      <Outlet />
    </div>
  </div>
);
