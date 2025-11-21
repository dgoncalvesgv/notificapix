import { Outlet } from "react-router-dom";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";
import { useAuthStore } from "../../store/auth";
import { Breadcrumbs } from "./Breadcrumbs";

export const AppShell = () => {
  const { user } = useAuthStore();
  return (
    <div className="flex bg-slate-50 dark:bg-slate-900 min-h-screen">
      <Sidebar role={user?.role} />
      <div className="flex-1 flex flex-col">
        <Topbar />
        <main className="flex-1 p-6 overflow-y-auto">
          <Breadcrumbs />
          <Outlet />
        </main>
      </div>
    </div>
  );
};
