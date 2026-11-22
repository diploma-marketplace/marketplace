import { Link } from 'react-router-dom';
import { Container, Button } from '@/shared/ui';
import { SearchBar } from '@/features/search';
import { usePreferences } from '@/app/providers/usePreferences';
import styles from './Header.module.css';

export function Header() {
  const { t, language, setLanguage, theme, setTheme } = usePreferences();

  return (
    <header className={styles.header}>
      <Container className={styles.inner}>
        <Link to="/" className={styles.logo} aria-label="FutureMarket, home page">
          <span className={styles.logoMark} aria-hidden="true" />
          <span>Future Market</span>
        </Link>

        <div className={styles.search}>
          <SearchBar />
        </div>

        <nav className={styles.actions} aria-label="actions">
          <Button variant="ghost">{t('login')}</Button>
          <Button variant="ghost">{t('signup')}</Button>

          <label className={styles.preference}>
            <span className="visually-hidden">{t('language')}</span>
            <select
              value={language}
              onChange={(e) => setLanguage(e.target.value as 'uk' | 'en')}
              aria-label={t('language')}
            >
              <option value="uk">UA</option>
              <option value="en">EN</option>
            </select>
          </label>

          <label className={styles.preference}>
            <span className="visually-hidden">{t('theme')}</span>
            <select
              value={theme}
              onChange={(e) =>
                setTheme(e.target.value as 'light' | 'dark' | 'system')
              }
              aria-label={t('theme')}
            >
              <option value="light">{t('light')}</option>
              <option value="dark">{t('dark')}</option>
              <option value="system">{t('system')}</option>
            </select>
          </label>
        </nav>
      </Container>
    </header>
  );
}