import {type ReactNode, useCallback, useEffect, useMemo, useState} from "react";
import {AuthContext, type AuthContextValue, type AuthStatus} from'./auth.context';
import {authApi, type LoginPayload, type RegisterPayload, type AuthSession} from '../api/authApi'

export function AuthProvider({children}: {children: ReactNode}){
    const [session, setSession] = useState<AuthSession | null>(null);
    const [status, setStatus] = useState<AuthStatus>('loading');

    useEffect(() => {
        const controller = new AbortController();

        authApi
            .me(controller.signal)
            .then((current) => {
                setSession(current);
                setStatus('authenticated');
            })
            .catch((error: unknown) => {
                if(error instanceof DOMException && error.name === "AbortError") return;
                setSession(null);
                setStatus('anonymous');
            });

        return () => controller.abort();
    }, []);

    const login = useCallback(async (payload: LoginPayload)=> {
        const next = await authApi.login(payload);
        setSession(next);
        setStatus('authenticated');
    }, []);

    const register = useCallback(async (payload: RegisterPayload)=> {
        const next = await authApi.register(payload);
        setSession(next);
        setStatus('authenticated');
    }, []);

    const logout = useCallback(async () => {
        try{
            await authApi.logout();
        } finally{
            setSession(null);
            setStatus('anonymous');
        }
    }, []);

    const value = useMemo<AuthContextValue>(
        () => ({ status, session, login, register, logout}),
        [status, session, login, register, logout],
    );

    return <AuthContext value={value}>{children}</AuthContext>
}