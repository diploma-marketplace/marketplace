import { createContext } from 'react';

export type Language = 'uk' | 'en';
export type Theme = 'light' | 'dark' | 'system';

export interface PreferencesContextType {
  language: Language;
  setLanguage: (language: Language) => void;
  theme: Theme;
  setTheme: (theme: Theme) => void;
}

export const PreferencesContext =
  createContext<PreferencesContextType | null>(null);