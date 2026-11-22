
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { PreferencesContext, type Language, type Theme } from './PreferencesContext';

export function PreferencesProvider({ children }: { children: ReactNode }) {
  const { t, i18n } = useTranslation();

  const [language, setLanguage] = useState<Language>(() => {
    return localStorage.getItem('marketplace-language') === 'en' ? 'en' : 'uk';
  });

  const [theme, setTheme] = useState<Theme>(() => {
    const saved = localStorage.getItem('marketplace-theme');
    return saved === 'light' || saved === 'dark' ? saved : 'system';
  });

  useEffect(() => {
    localStorage.setItem('marketplace-language', language);
    document.documentElement.lang = language;
    void i18n.changeLanguage(language);
  }, [language, i18n]);

  useEffect(() => {
    localStorage.setItem('marketplace-theme', theme);

    const applyTheme = () => {
      const actualTheme =
        theme === 'system'
          ? window.matchMedia('(prefers-color-scheme: dark)').matches
            ? 'dark'
            : 'light'
          : theme;

      document.documentElement.dataset.theme = actualTheme;
    };

    applyTheme();

    const media = window.matchMedia('(prefers-color-scheme: dark)');
    media.addEventListener('change', applyTheme);

    return () => media.removeEventListener('change', applyTheme);
  }, [theme]);

  const value = useMemo(
    () => ({
      language,
      setLanguage,
      theme,
      setTheme,
      t: (key: string) => String(t(key)),
    }),
    [language, theme, t],
  );

  return (
    <PreferencesContext.Provider value={value}>
      {children}
    </PreferencesContext.Provider>
  );
}
