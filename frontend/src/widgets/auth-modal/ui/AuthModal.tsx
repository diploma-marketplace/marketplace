import { Modal } from '@/shared/ui';
import { LoginForm, RegisterForm, useAuthModal } from '@/features/auth';

export function AuthModal() {
    const { view, close } = useAuthModal();

    return (
        <Modal
            open={view !== null}
            onClose={close}
            title={view === 'register' ? 'Create an account' : 'Log in'}
        >
            {view === 'login' && <LoginForm />}
            {view === 'register' && <RegisterForm />}
        </Modal>
    );
}
