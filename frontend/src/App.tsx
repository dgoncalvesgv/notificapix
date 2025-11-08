import { Navigate, Outlet, Route, Routes } from "react-router-dom";
import { AuthShell } from "./components/layout/AuthShell";
import { AppShell } from "./components/layout/AppShell";
import { LoginPage } from "./pages/auth/LoginPage";
import { RegisterPage } from "./pages/auth/RegisterPage";
import { ForgotPasswordPage } from "./pages/auth/ForgotPasswordPage";
import { ResetPasswordPage } from "./pages/auth/ResetPasswordPage";
import { OverviewPage } from "./pages/app/overview/OverviewPage";
import { TransactionsPage } from "./pages/app/transactions/TransactionsPage";
import { AlertsPage } from "./pages/app/alerts/AlertsPage";
import { BankConnectionsPage } from "./pages/app/bank/BankConnectionsPage";
import { NotificationSettingsPage } from "./pages/app/settings/NotificationSettingsPage";
import { TeamPage } from "./pages/app/team/TeamPage";
import { BillingPage } from "./pages/app/billing/BillingPage";
import { AuditLogsPage } from "./pages/app/logs/AuditLogsPage";
import { ApiKeysPage } from "./pages/app/api/ApiKeysPage";
import { useAuthStore } from "./store/auth";
import { ToastProvider } from "./context/ToastContext";

const RequireAuth = () => {
  const token = useAuthStore((state) => state.token);
  return token ? <Outlet /> : <Navigate to="/login" replace />;
};

const RequireGuest = () => {
  const token = useAuthStore((state) => state.token);
  return token ? <Navigate to="/app/overview" replace /> : <Outlet />;
};

export default function App() {
  return (
    <ToastProvider>
      <Routes>
        <Route element={<RequireGuest />}>
          <Route element={<AuthShell />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/forgot" element={<ForgotPasswordPage />} />
            <Route path="/reset" element={<ResetPasswordPage />} />
          </Route>
        </Route>
        <Route element={<RequireAuth />}>
          <Route element={<AppShell />}>
            <Route path="/app/overview" element={<OverviewPage />} />
            <Route path="/app/transactions" element={<TransactionsPage />} />
            <Route path="/app/alerts" element={<AlertsPage />} />
            <Route path="/app/bank-connections" element={<BankConnectionsPage />} />
            <Route path="/app/settings/notifications" element={<NotificationSettingsPage />} />
            <Route path="/app/team" element={<TeamPage />} />
            <Route path="/app/billing" element={<BillingPage />} />
            <Route path="/app/audit-logs" element={<AuditLogsPage />} />
            <Route path="/app/api-keys" element={<ApiKeysPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </ToastProvider>
  );
}
