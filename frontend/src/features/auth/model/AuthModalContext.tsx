import { useMemo, useState, type ReactNode} from 'react';
import { AuthModalContext, type AuthModalContextValue, type AuthModalView} from './auth-modal.context'

export function AuthModalProvider({ children}: { children: ReactNode }) {
    const [view, setView] = useState<AuthModalView | null>(null)

    const value = useMemo<AuthModalContextValue>(
        () => ({
            view,
            openLogin: () => setView('login'),
            openRegister: () => setView('register'),
            close: () => setView(null),
        }),
        [view],
    );

    return <AuthModalContext value={value}>{children}</AuthModalContext>
}