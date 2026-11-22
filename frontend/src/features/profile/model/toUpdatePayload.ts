import type { UpdateProfilePayload, UserProfile } from '../api/profileApi';

export function toUpdatePayload(
  profile: UserProfile,
  overrides: Partial<UpdateProfilePayload> = {},
): UpdateProfilePayload {
  return {
    fullName: profile.fullName,
    phoneNumber: profile.phoneNumber ?? null,
    avatarUrl: profile.avatarUrl ?? null,
    companyName: profile.companyName ?? null,
    address: profile.address ?? null,
    websiteUrl: profile.websiteUrl ?? null,
    socialLinks: {
      telegram: profile.socialLinks?.telegram ?? null,
      instagram: profile.socialLinks?.instagram ?? null,
      facebook: profile.socialLinks?.facebook ?? null,
    },
    preferredLanguage: profile.preferredLanguage,
    preferredTheme: profile.preferredTheme,
    ...overrides,
  };
}
