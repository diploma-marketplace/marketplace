export type FieldName = 'email' | 'password' | 'fullName' | 'confirmPassword';
export type FieldErrors = Partial<Record<FieldName, string>>;

const EMAIL_REGEX = /^\S+@\S+\.\S+$/;

const PASSWORD_MIN = 8;
const PASSWORD_MAX = 128;
const NAME_MIN = 2;
const NAME_MAX = 150;

const HAS_LOWERCASE = /\p{Ll}/u;
const HAS_UPPERCASE = /\p{Lu}/u;
const HAS_DIGIT = /\p{Nd}/u;
const HAS_SPECIAL = /[^\p{L}\p{N}]/u;

export function hasErrors(errors: FieldErrors): boolean {
  return Object.keys(errors).length > 0;
}

function validateEmail(email: string): string | undefined {
  if (!email.trim()) return 'Enter your email.';
  if (!EMAIL_REGEX.test(email.trim())) return 'Enter a valid email.';
}

function validatePassword(password: string): string | undefined {
  if (!password) return 'Enter your password.';
  if (password.length < PASSWORD_MIN) return `At least ${PASSWORD_MIN} characters required.`;
  if (password.length > PASSWORD_MAX) return `At most ${PASSWORD_MAX} characters allowed.`;
  if (!HAS_LOWERCASE.test(password)) return 'Add a lowercase letter.';
  if (!HAS_UPPERCASE.test(password)) return 'Add an uppercase letter.';
  if (!HAS_DIGIT.test(password)) return 'Add a digit.';
  if (!HAS_SPECIAL.test(password)) return 'Add a special symbol (e.g. ! @ # $ ?).';
}

export function validateLogin(values: { email: string; password: string }): FieldErrors {
  const errors: FieldErrors = {};
  const email = validateEmail(values.email);
  if (email) errors.email = email;
  if (!values.password) errors.password = 'Enter your password';
  return errors;
}

export function validateRegister(values: {
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
}): FieldErrors {
  const errors: FieldErrors = {};
  const name = values.fullName.trim();

  if (!name) errors.fullName = 'Enter your full name.';
  else if (name.length < NAME_MIN) errors.fullName = `At least ${NAME_MIN} characters required.`;
  else if (name.length > NAME_MAX) errors.fullName = `At most ${NAME_MAX} characters required.`;

  const email = validateEmail(values.email);
  if (email) errors.email = email;

  const password = validatePassword(values.password);
  if (password) errors.password = password;

  if (!values.confirmPassword) errors.confirmPassword = 'Repeat your password.';
  else if (values.confirmPassword !== values.password)
    errors.confirmPassword = 'Passwords do not match.';

  return errors;
}
