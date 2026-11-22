import { ApiError } from '@/shared/api';
import type { FieldErrors, FieldName } from './validation';

export interface MappedAuthError {
  formError: string | null;
  fieldErrors: FieldErrors;
}

const FIELD_NAMES: FieldName[] = ['email', 'password', 'fullName'];

function isFieldName(key: string): key is FieldName {
  return (FIELD_NAMES as string[]).includes(key);
}

export function mapAuthError(error: unknown): MappedAuthError {
  if (!(error instanceof ApiError)) {
    return { formError: 'Something went wrong. Try again.', fieldErrors: {} };
  }

  switch (error.code) {
    case 'INVALID_CREDENTIALS':
      return { formError: 'Invalid email or password.', fieldErrors: {} };
    case 'ACCOUNT_BLOCKED':
      return {
        formError: 'Your account is temporarily blocked. Try again later.',
        fieldErrors: {},
      };
    case 'EMAIL_TAKEN':
      return {
        formError: null,
        fieldErrors: { email: 'An account with this email already exists.' },
      };
  }

  const fieldErrors: FieldErrors = {};
  const unplaced: string[] = [];

  for (const [key, messages] of Object.entries(error.fieldErrors)) {
    if (isFieldName(key) && messages[0]) fieldErrors[key] = messages[0];
    else unplaced.push(...messages);
  }

  const hasFieldErrors = Object.keys(fieldErrors).length > 0;
  return {
    formError: unplaced.length > 0 ? unplaced.join(' ') : hasFieldErrors ? null : error.message,
    fieldErrors,
  };
}
