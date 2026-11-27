import type { UpdateProfilePayload, UserProfile } from '../api/profileApi';

export interface ContactsValues {
  fullName: string;
  phoneNumber: string;
  address: string;
  telegram: string;
  instagram: string;
  facebook: string;
  companyName: string;
  websiteUrl: string;
}

export type ContactsErrors = Partial<Record<keyof ContactsValues, string>>;

const NAME_MIN = 2;
const NAME_MAX = 150;
const PHONE_MAX = 32;
const ADDRESS_MAX = 300;
const SOCIAL_MAX = 200;
const COMPANY_MAX = 200;
const URL_MAX = 500;

const PHONE_RE = /^\+?[\d\s().-]{7,}$/;

export function toContactsValues(profile: UserProfile): ContactsValues {
  return {
    fullName: profile.fullName,
    phoneNumber: profile.phoneNumber ?? '',
    address: profile.address ?? '',
    telegram: profile.socialLinks?.telegram ?? '',
    instagram: profile.socialLinks?.instagram ?? '',
    facebook: profile.socialLinks?.facebook ?? '',
    companyName: profile.companyName ?? '',
    websiteUrl: profile.websiteUrl ?? '',
  };
}

function isHttpUrl(value: string): boolean {
  try {
    const url = new URL(value);
    return url.protocol === 'http:' || url.protocol === 'https:';
  } catch {
    return false;
  }
}

export function validateContacts(
  values: ContactsValues,
  options: { isCompany: boolean },
): ContactsErrors {
  const errors: ContactsErrors = {};

  const name = values.fullName.trim();
  if (!name) errors.fullName = 'Enter your name.';
  else if (name.length < NAME_MIN) errors.fullName = `At least ${NAME_MIN} characters.`;
  else if (name.length > NAME_MAX) errors.fullName = `At most ${NAME_MAX} characters.`;

  const phone = values.phoneNumber.trim();
  if (phone && (phone.length > PHONE_MAX || !PHONE_RE.test(phone)))
    errors.phoneNumber = 'Enter a valid phone number, e.g. +380501234567.';

  if (values.address.trim().length > ADDRESS_MAX)
    errors.address = `At most ${ADDRESS_MAX} characters.`;

  for (const key of ['telegram', 'instagram', 'facebook'] as const) {
    if (values[key].trim().length > SOCIAL_MAX) errors[key] = `At most ${SOCIAL_MAX} characters.`;
  }

  if (options.isCompany) {
    if (values.companyName.trim().length > COMPANY_MAX)
      errors.companyName = `At most ${COMPANY_MAX} characters.`;

    const website = values.websiteUrl.trim();
    if (website && (website.length > URL_MAX || !isHttpUrl(website)))
      errors.websiteUrl = 'Enter a link starting with http:// or https://.';
  }

  return errors;
}

/* Edited form values -> the fields of the update payload they replace. */
export function toContactsOverrides(
  values: ContactsValues,
  options: { isCompany: boolean },
): Partial<UpdateProfilePayload> {
  return {
    fullName: values.fullName.trim(),
    // the server turns a blank phone into "no phone", but its [Phone] check rejects ""
    phoneNumber: values.phoneNumber.trim() || null,
    // blank strings clear these fields on the server
    address: values.address.trim(),
    socialLinks: {
      telegram: values.telegram.trim(),
      instagram: values.instagram.trim(),
      facebook: values.facebook.trim(),
    },
    ...(options.isCompany
      ? {
          companyName: values.companyName.trim(),
          // null means "leave as is": the server's [Url] check rejects ""
          websiteUrl: values.websiteUrl.trim() || null,
        }
      : {}),
  };
}
