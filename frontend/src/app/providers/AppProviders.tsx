import type { ReactNode } from 'react';
import { PreferencesProvider } from './PreferencesProvider';

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <PreferencesProvider>
      {children}
    </PreferencesProvider>
  );
}