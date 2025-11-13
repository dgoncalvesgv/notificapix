export type ApiResponse<T> = {
  success: boolean;
  data?: T;
  error?: string;
  code?: string;
};

export type AuthResponse = {
  token: string;
  user: UserDto;
  organization: OrganizationDto;
};

export type UserDto = {
  id: string;
  email: string;
  role: "OrgAdmin" | "OrgMember";
};

export type OrganizationDto = {
  id: string;
  name: string;
  slug: string;
  plan: "Starter" | "Pro" | "Business";
  usageCount: number;
  quota: number;
  billingEmail: string;
};

export type PixTransactionDto = {
  id: string;
  txId: string;
  endToEndId: string;
  amount: number;
  occurredAt: string;
  payerName: string;
  payerKey: string;
  description: string;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type AlertDto = {
  id: string;
  channel: "Email" | "Webhook";
  status: "Pending" | "Sent" | "Failed";
  lastAttemptAt?: string;
  attempts: number;
  payloadJson: string;
};

export type BankConnectionDto = {
  id: string;
  provider: "Mock" | "Pluggy" | "Belvo";
  consentId: string;
  status: "Pending" | "Active" | "Revoked" | "Error";
  connectedAt?: string;
};

export type NotificationSettingsDto = {
  emails: string[];
  webhookUrl?: string;
  webhookSecret?: string;
  enabled: boolean;
};

export type TeamMemberDto = {
  membershipId: string;
  userId: string;
  email: string;
  role: UserDto["role"];
  joinedAt: string;
};

export type ApiKeyDto = {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  lastUsedAt?: string;
};

export type ApiKeyCreatedResponse = {
  key: ApiKeyDto;
  secret: string;
};

export type BillingSessionResponse = {
  url: string;
};

export type OverviewMetricsResponse = {
  todayTotal: number;
  last7DaysTotal: number;
  last30DaysTotal: number;
  recentTransactions: PixTransactionDto[];
  alertsSentToday: number;
  activeBankConnections: number;
};

export type UsageResponse = {
  usageCount: number;
  quota: number;
  usageMonthStartsAt: string;
};

export type AuditLogDto = {
  id: string;
  action: string;
  dataJson: string;
  createdAt: string;
};

export type BankIntegrationStatusDto = {
  configured: boolean;
  updatedAt?: string;
  integrationId?: string;
  bank?: string;
  createdAt?: string;
  productionEnabled?: boolean;
  lastTestedAt?: string;
  serviceUrl?: string;
  apiKey?: string;
  accountIdentifier?: string;
  sandboxClientId?: string;
  sandboxClientSecret?: string;
  productionClientId?: string;
  productionClientSecret?: string;
  certificatePassword?: string;
  certificateFileName?: string;
  hasCertificate?: boolean;
};

export type ItauIntegrationPayload = {
  sandboxClientId: string;
  sandboxClientSecret: string;
  productionClientId: string;
  productionClientSecret: string;
  certificatePassword: string;
  certificateFileName: string;
  certificateBase64: string;
  productionEnabled: boolean;
  serviceUrl: string;
  apiKey: string;
  accountIdentifier: string;
};

export type BankSyncResultDto = {
  integrationsProcessed: number;
  transactionsImported: number;
  message: string;
};
