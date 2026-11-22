import type { ContactsErrors, ContactsValues } from './contacts';
import { ApiError } from '@/shared/api';

export interface MappedProfileError {
  formError: string | null;
  fieldErrors: ContactsErrors;
}

const FIELD_BY_SERVER_KEY: Record<string, keyof ContactsValues> = {
  fullName: 'fullName',
  phonenumber: 'phoneNumber',
  address: 'address',
  companyname: 'companyName',
  websiteurl: 'websiteUrl',
  'sociallinks.telegram': 'telegram',
  'sociallinks.instagram': 'instagram',
  'sociallinks.facebook': 'facebook',
};

export function mapProfileError(error: unknown): MappedProfileError {
  if (!(error instanceof ApiError)) {
    return { formError: 'Something went wrong. Try again.', fieldErrors: {} };
  }

  if (error.status === 401) {
    return { formError: 'Your session has expired. Log in again.', fieldErrors: {} };
  }

  const fieldErrors: ContactsErrors = {};
  const unplaced: string[] = [];

  for (const [key, messages] of Object.entries(error.fieldErrors)) {
    const field = FIELD_BY_SERVER_KEY[key.toLowerCase()];
    if (field && messages[0]) fieldErrors[field] = messages[0];
    else unplaced.push(...messages);
  }

  const hasFieldErrors = Object.keys(fieldErrors).length > 0;
  return {
    formError: unplaced.length > 0 ? unplaced.join(' ') : hasFieldErrors ? null : error.message,
    fieldErrors,
  };
}
