import {
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { useTranslation } from 'react-i18next';
import {
  PreferencesContext,
  type Language,
  type Theme,
} from './PreferencesContext';

export function PreferencesProvider({
  children,
}: {
  children: ReactNode;
}) {
  const { i18n } = useTranslation();

  const [language, setLanguage] = useState<Language>(() => {
    return localStorage.getItem('marketplace-language') === 'en'
      ? 'en'
      : 'uk';
  });

  const [theme, setTheme] = useState<Theme>(() => {
    const savedTheme = localStorage.getItem('marketplace-theme');

    return savedTheme === 'light' || savedTheme === 'dark'
      ? savedTheme
      : 'system';
  });

  useEffect(() => {
    localStorage.setItem('marketplace-language', language);
    document.documentElement.lang = language;
    void i18n.changeLanguage(language);
  }, [language, i18n]);

  useEffect(() => {
    localStorage.setItem('marketplace-theme', theme);

    const media = window.matchMedia('(prefers-color-scheme: dark)');

    const applyTheme = () => {
      const actualTheme =
        theme === 'system'
          ? media.matches
            ? 'dark'
            : 'light'
          : theme;

      document.documentElement.dataset.theme = actualTheme;
    };

    applyTheme();
    media.addEventListener('change', applyTheme);

    return () => {
      media.removeEventListener('change', applyTheme);
    };
  }, [theme]);

  const value = useMemo(
    () => ({
      language,
      setLanguage,
      theme,
      setTheme,
    }),
    [language, theme],
  );

  return (
    <PreferencesContext.Provider value={value}>
      {children}
    </PreferencesContext.Provider>
  );
}