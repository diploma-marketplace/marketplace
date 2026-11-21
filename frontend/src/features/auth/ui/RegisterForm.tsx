import { useState, type FormEvent } from 'react';
import { Button, TextField } from '@/shared/ui';
import { useAuth } from '../model/useAuth';
import { useAuthModal } from '../model/useAuthModal';
import { mapAuthError } from '../model/mapAuthError';
import { hasErrors, validateRegister, type FieldErrors } from '../model/validation';
import styles from './AuthForm.module.css';

export function RegisterForm() {
    const { register } = useAuth();
    const { close, openLogin } = useAuthModal();

    const [fullName, setFullName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
    const [formError, setFormError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        if (submitting) return;

        const errors = validateRegister({ fullName, email, password, confirmPassword });
        setFieldErrors(errors);
        setFormError(null);
        if (hasErrors(errors)) return;

        setSubmitting(true);
        try {
            await register({ fullName: fullName.trim(), email: email.trim(), password, role: 'User' });
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
                label="Full name"
                name="name"
                autoComplete="name"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                error={fieldErrors.fullName}
            />
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
                autoComplete="new-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                error={fieldErrors.password}
            />
            <TextField
                label="Confirm password"
                type="password"
                name="confirmPassword"
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                error={fieldErrors.confirmPassword}
            />

            {formError && (
                <p className={styles.formError} role="alert">
                    {formError}
                </p>
            )}

            <Button variant="primary" type="submit" className={styles.submit} disabled={submitting}>
                {submitting ? 'Creating account...' : 'Register'}
            </Button>

            <button type="button" className={styles.link} onClick={openLogin}>
                Already have an account? Login
            </button>
        </form>
    );
}
