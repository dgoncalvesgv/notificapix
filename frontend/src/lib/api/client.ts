import axios from "axios";
import type {
  ApiKeyDto,
  ApiResponse,
  AuthResponse,
  BillingSessionResponse,
  NotificationSettingsDto,
  OverviewMetricsResponse,
  PixTransactionDto,
  PagedResult,
  AlertDto,
  TeamMemberDto,
  OrganizationDto,
  BankConnectionDto,
  UsageResponse,
  ApiKeyCreatedResponse
} from "./types";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5089",
  withCredentials: true
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("np_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const authApi = {
  login: (payload: { email: string; password: string }) => api.post<ApiResponse<AuthResponse>>("/auth/login", payload),
  register: (payload: { name: string; email: string; password: string; organizationName: string }) =>
    api.post<ApiResponse<AuthResponse>>("/auth/register", payload),
  forgot: (payload: { email: string }) => api.post<ApiResponse<string>>("/auth/forgot", payload)
};

export const dashboardApi = {
  overview: () => api.get<ApiResponse<OverviewMetricsResponse>>("/app/overview"),
  quota: () => api.get<ApiResponse<UsageResponse>>("/quota/usage")
};

export const transactionsApi = {
  list: (payload: Record<string, unknown>) => api.post<ApiResponse<PagedResult<PixTransactionDto>>>("/transactions/list", payload)
};

export const alertsApi = {
  list: (payload: Record<string, unknown>) => api.post<ApiResponse<PagedResult<AlertDto>>>("/alerts/list", payload),
  test: (payload: { amount: number; payerName: string; payerKey: string; description: string }) =>
    api.post<ApiResponse<AlertDto>>("/alerts/test", payload)
};

export const settingsApi = {
  getNotifications: () => api.get<ApiResponse<NotificationSettingsDto>>("/settings/notifications"),
  updateNotifications: (payload: NotificationSettingsDto) => api.put<ApiResponse<NotificationSettingsDto>>("/settings/notifications", payload)
};

export const teamApi = {
  list: () => api.get<ApiResponse<TeamMemberDto[]>>("/team/list"),
  invite: (payload: { email: string; role: string }) => api.post<ApiResponse<string>>("/team/invite", payload)
};

export const billingApi = {
  checkout: (payload: { plan: string }) => api.post<ApiResponse<BillingSessionResponse>>("/billing/checkout-session", payload),
  portal: () => api.post<ApiResponse<BillingSessionResponse>>("/billing/portal")
};

export const orgApi = {
  current: () => api.get<ApiResponse<OrganizationDto>>("/org/current")
};

export const bankApi = {
  list: () => api.get<ApiResponse<BankConnectionDto[]>>("/bank/connections/list"),
  connect: () => api.post<ApiResponse<string>>("/bank/connect/init", {}),
  revoke: (connectionId: string) => api.post<ApiResponse<string>>("/bank/connections/revoke", { connectionId })
};

export const apiKeysApi = {
  list: () => api.get<ApiResponse<ApiKeyDto[]>>("/apikeys"),
  create: (payload: { name: string }) => api.post<ApiResponse<ApiKeyCreatedResponse>>("/apikeys/create", payload)
};

export { api };
