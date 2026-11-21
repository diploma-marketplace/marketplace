import {useContext} from "react";
import {AuthModalContext} from './auth-modal.context';

export function useAuthModal(){
    const ctx = useContext(AuthModalContext);
    if(!ctx) throw new Error("useAuthModal must be inside <AuthModalProvider>");
    return ctx;
}