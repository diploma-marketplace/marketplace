
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

import uk from './uk.json';
import en from './en.json';

const savedLanguage = localStorage.getItem('marketplace-language');

i18n.use(initReactI18next).init({
  resources: {
    uk: { translation: uk },
    en: { translation: en },
  },
  lng: savedLanguage === 'en' ? 'en' : 'uk',
  fallbackLng: 'uk',
  interpolation: {
    escapeValue: false,
  },
});

export default i18n;
