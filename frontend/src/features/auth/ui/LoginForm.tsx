import { useState, type FormEvent } from 'react';
import { Button, TextField } from '@/shared/ui';
import { useAuth } from '../model/useAuth';
import { useAuthModal } from '../model/useAuthModal';
import { mapAuthError } from '../model/mapAuthError';
import { hasErrors, validateLogin, type FieldErrors } from '../model/validation';
import styles from './AuthForm.module.css';

export function LoginForm() {
  const { login } = useAuth();
  const { close, openRegister } = useAuthModal();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (submitting) return;

    const errors = validateLogin({ email, password });
    setFieldErrors(errors);
    setFormError(null);
    if (hasErrors(errors)) return;

    setSubmitting(true);
    try {
      await login({ email: email.trim(), password });
      close();
    } catch (error) {
      const mapped = mapAuthError(error);
      setFieldErrors(mapped.fieldErrors);
      setFormError(mapped.formError);
      setSubmitting(false);
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <TextField
        label="Email"
        type="email"
        name="email"
        autoComplete="email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        error={fieldErrors.email}
      />
      <TextField
        label="Password"
        type="password"
        name="password"
        autoComplete="current-password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        error={fieldErrors.password}
      />

      {/* password recovery isn't implemented on the backend yet */}
      <button type="button" className={styles.link} disabled title="Coming soon">
        Forgot password?
      </button>

      {formError && (
        <p className={styles.formError} role="alert">
          {formError}
        </p>
      )}

      <Button variant="primary" type="submit" className={styles.submit} disabled={submitting}>
        {submitting ? 'Logging in…' : 'Login'}
      </Button>

      <button type="button" className={styles.link} onClick={openRegister}>
        Don’t have an account? Register
      </button>
    </form>
  );
}
