
import { createContext } from 'react';

export type Language = 'uk' | 'en';
export type Theme = 'light' | 'dark' | 'system';

export type PreferencesContextValue = {
  language: Language;
  setLanguage: (language: Language) => void;
  theme: Theme;
  setTheme: (theme: Theme) => void;
  t: (key: string) => string;
};

export const PreferencesContext =
  createContext<PreferencesContextValue | null>(null);
