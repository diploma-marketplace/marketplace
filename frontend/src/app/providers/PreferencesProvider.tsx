import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';

type Language = 'uk' | 'en';
type Theme = 'light' | 'dark' | 'system';

type PreferencesContextValue = {
  language: Language;
  setLanguage: (language: Language) => void;
  theme: Theme;
  setTheme: (theme: Theme) => void;
  t: (key: string) => string;
};

const translations: Record<Language, Record<string, string>> = {
  uk: {
    home: 'Головна',
    login: 'Увійти',
    signup: 'Реєстрація',
    search: 'Пошук...',
    searchLabel: 'Пошук',
    categories: 'Категорії',
    recommended: 'Рекомендовані товари',
    promotions: 'Акції',
    marketplace: 'Від маркетплейсу',
    company: 'Від компанії',
    'third-party': 'Від продавця',
    electronics: 'Електроніка',
    appliances: 'Побутова техніка',
    fashion: 'Одяг',
    home_garden: 'Дім та сад',
    sports: 'Спорт',
    kids: 'Дитячі товари',
    auto: 'Авто',
    previousSlide: 'Попередній слайд',
    nextSlide: 'Наступний слайд',
    goToSlide: 'Перейти до слайду',
    language: 'Мова',
    theme: 'Тема',
    light: 'Світла',
    dark: 'Темна',
    system: 'Системна',
    footer: '© Future Market',
  },
  en: {
    home: 'Home',
    login: 'Log In',
    signup: 'Sign Up',
    search: 'Search...',
    searchLabel: 'Search',
    categories: 'Categories',
    recommended: 'Recommended products',
    promotions: 'Promotions',
    marketplace: 'From marketplace',
    company: 'From a company',
    'third-party': 'From a seller',
    electronics: 'Electronics',
    appliances: 'Appliances',
    fashion: 'Fashion',
    home_garden: 'Home & Garden',
    sports: 'Sports',
    kids: 'Kids',
    auto: 'Auto',
    previousSlide: 'Previous slide',
    nextSlide: 'Next slide',
    goToSlide: 'Go to slide',
    language: 'Language',
    theme: 'Theme',
    light: 'Light',
    dark: 'Dark',
    system: 'System',
    footer: '© Future Market',
  },
};

const PreferencesContext = createContext<PreferencesContextValue | null>(null);

export function PreferencesProvider({ children }: { children: ReactNode }) {
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
  }, [language]);

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

  const value = useMemo(() => ({
    language,
    setLanguage,
    theme,
    setTheme,
    t: (key: string) => translations[language][key] ?? key,
  }), [language, theme]);

  return (
    <PreferencesContext.Provider value={value}>
      {children}
    </PreferencesContext.Provider>
  );
}

export function usePreferences() {
  const context = useContext(PreferencesContext);

  if (!context) {
    throw new Error('usePreferences must be used inside PreferencesProvider');
  }

  return context;
}