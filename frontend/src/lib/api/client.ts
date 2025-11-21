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
  ApiKeyCreatedResponse,
  BankIntegrationStatusDto,
  ItauIntegrationPayload,
  BankSyncResultDto,
  StripeSubscriptionResponse,
  PlanInfoDto,
  PixReceiversResponse,
  PixStaticQrCodeDto
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
  forgot: (payload: { email: string }) => api.post<ApiResponse<string>>("/auth/forgot", payload),
  changePassword: (payload: { currentPassword: string; newPassword: string }) =>
    api.post<ApiResponse<string>>("/auth/change-password", payload)
};

export const dashboardApi = {
  overview: () => api.get<ApiResponse<OverviewMetricsResponse>>("/app/overview"),
  quota: () => api.get<ApiResponse<UsageResponse>>("/quota/usage")
};

export const transactionsApi = {
  list: (payload: Record<string, unknown>) => api.post<ApiResponse<PagedResult<PixTransactionDto>>>("/transactions/list", payload),
  syncBanks: () => api.get<ApiResponse<BankSyncResultDto>>("/transactions/sync-banks")
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
  checkout: (payload: { plan: string }) => api.post<ApiResponse<StripeSubscriptionResponse>>("/billing/checkout-session", payload),
  portal: () => api.post<ApiResponse<BillingSessionResponse>>("/billing/portal"),
  plans: () => api.get<ApiResponse<PlanInfoDto[]>>("/billing/plans")
};

export const orgApi = {
  current: () => api.get<ApiResponse<OrganizationDto>>("/org/current"),
  update: (payload: { name: string; billingEmail: string }) => api.put<ApiResponse<OrganizationDto>>("/org/update", payload)
};

export const pixApi = {
  listKeys: () => api.get<ApiResponse<PixReceiversResponse>>("/pix/keys"),
  createKey: (payload: { label: string; keyType: string; keyValue: string }) => api.post<ApiResponse<PixReceiverDto>>("/pix/keys", payload),
  selectKey: (payload: { pixKeyId: string }) => api.post<ApiResponse<string>>("/pix/keys/select", payload),
  deleteKey: (pixKeyId: string) => api.delete<ApiResponse<string>>(`/pix/keys/${pixKeyId}`),
  listQrCodes: (params?: { description?: string; createdFrom?: string; createdTo?: string; sortBy?: string; sortDirection?: string }) =>
    api.get<ApiResponse<PixStaticQrCodeDto[]>>("/pix/qrcodes", { params }),
  createQrCode: (payload: { amount: number; pixKeyId?: string; description?: string }) =>
    api.post<ApiResponse<PixStaticQrCodeDto>>("/pix/qrcodes", payload),
  deleteQrCode: (qrCodeId: string) => api.delete<ApiResponse<string>>(`/pix/qrcodes/${qrCodeId}`)
};

export const bankApi = {
  list: () => api.get<ApiResponse<BankConnectionDto[]>>("/bank/connections/list"),
  connect: () => api.post<ApiResponse<string>>("/bank/connect/init", {}),
  revoke: (connectionId: string) => api.post<ApiResponse<string>>("/bank/connections/revoke", { connectionId }),
  getItauIntegration: () => api.get<ApiResponse<BankIntegrationStatusDto>>("/bank/api/itau"),
  saveItauIntegration: (payload: ItauIntegrationPayload) => api.post<ApiResponse<BankIntegrationStatusDto>>("/bank/api/itau", payload),
  testItauIntegration: (payload: { useProduction?: boolean }) =>
    api.post<ApiResponse<BankIntegrationStatusDto>>("/bank/api/itau/test", payload)
};

export const apiKeysApi = {
  list: () => api.get<ApiResponse<ApiKeyDto[]>>("/apikeys"),
  create: (payload: { name: string }) => api.post<ApiResponse<ApiKeyCreatedResponse>>("/apikeys/create", payload)
};

export { api };
