import type { ReactNode } from 'react';
import { AuthModalProvider, AuthProvider } from '@/features/auth';

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <AuthProvider>
      <AuthModalProvider>{children}</AuthModalProvider>
    </AuthProvider>
  );
}
