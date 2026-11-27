import { request } from '@/shared/api';

export type InterfaceLanguage = 'Uk' | 'En';
export type ThemeMode = 'Light' | 'Dark' | 'System';

export interface SocialLinks {
  telegram?: string | null;
  instagram?: string | null;
  facebook?: string | null;
}

/** Mirrors UserProfileResponse from the backend. */
export interface UserProfile {
  id: number;
  email: string;
  isEmailVerified: boolean;
  fullName: string;
  phoneNumber?: string | null;
  isPhoneVerified: boolean;
  avatarUrl?: string | null;
  role: 'User' | 'Company' | 'Admin';
  accountStatus: string;
  verificationStatus: string;
  isSoleProprietor: boolean;
  companyName?: string | null;
  taxRegistrationCode?: string | null;
  address?: string | null;
  websiteUrl?: string | null;
  socialLinks?: SocialLinks | null;
  rating: number;
  reviewsCount: number;
  activeListingsCount: number;
  maxActiveListingsLimit: number;
  preferredLanguage: InterfaceLanguage;
  preferredTheme: ThemeMode;
  createdAt: string;
}

/** Mirrors UpdateProfileRequest.*/
export interface UpdateProfilePayload {
  fullName: string;
  phoneNumber?: string | null;
  avatarUrl?: string | null;
  companyName?: string | null;
  address?: string | null;
  websiteUrl?: string | null;
  socialLinks?: SocialLinks | null;
  preferredLanguage: InterfaceLanguage;
  preferredTheme: ThemeMode;
}

export const profileApi = {
  getMe: (signal?: AbortSignal) => request<UserProfile>('/users/me', { signal }),
  updateMe: (body: UpdateProfilePayload) =>
    request<UserProfile>('/users/me', { method: 'PUT', body }),
};
