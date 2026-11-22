import { request } from '@/shared/api';

export type UserRole = 'User' | 'Company' | 'Marketplace';

export type AccountStatus =
  | 'PendingContactVerification'
  | 'Active'
  | 'TemporarilyBlocked'
  | 'PermanentlyBlocked'
  | 'Deactivated';

export interface AuthSession {
  userId: number;
  email: string;
  fullName: string;
  avatarUrl?: string | null;
  role: UserRole;
  accountStatus: AccountStatus;
  verificationStatus: string;
  preferredLanguage: string;
  preferredTheme: string;
  expiresAt: number;
}

export interface LoginPayload {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterPayload {
  email: string;
  password: string;
  fullName: string;
  role: UserRole;
}

export const authApi = {
  login: (body: LoginPayload) => request<AuthSession>('/auth/login', { method: 'POST', body }),
  register: (body: RegisterPayload) =>
    request<AuthSession>('/auth/register', { method: 'POST', body }),
  logout: () => request<{ message: string }>('/auth/logout', { method: 'POST' }),
  me: (signal?: AbortSignal) => request<AuthSession>('/auth/me', { signal }),
};
