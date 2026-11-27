import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Container, Button } from '@/shared/ui';
import { SearchBar } from '@/features/search';
import styles from './Header.module.css';
import { useAuth, useAuthModal } from '@/features/auth';
import { usePreferences } from '@/app/providers/preferences/usePreferences';

export function Header() {
  const { status, session, logout } = useAuth();
  const { openLogin, openRegister } = useAuthModal();
  const { t } = useTranslation();
  const { language, setLanguage, theme, setTheme } = usePreferences();

  return (
    <header className={styles.header}>
      <Container className={styles.inner}>
        <Link to="/" className={styles.logo} aria-label="FutureMarket, home page">
          <span className={styles.logoMark} aria-hidden="true" />
          <span className={styles.logo}>Future Market</span>
        </Link>

        <div className={styles.search}>
          <SearchBar />
        </div>

        <nav className={styles.actions} aria-label="actions">
          <div className={styles.preferences}>
            <label className={styles.preferenceControl}>
              <span className={styles.visuallyHidden}>
                {t('preferences.language')}
              </span>
              <select
                value={language}
                onChange={(event) =>
                  setLanguage(event.target.value as 'uk' | 'en')
                }
                aria-label={t('preferences.language')}
              >
                <option value="uk">UA</option>
                <option value="en">EN</option>
              </select>
            </label>

            <label className={styles.preferenceControl}>
              <span className={styles.visuallyHidden}>
                {t('preferences.theme')}
              </span>
              <select
                value={theme}
                onChange={(event) =>
                  setTheme(event.target.value as 'light' | 'dark' | 'system')
                }
                aria-label={t('preferences.theme')}
              >
                <option value="light">Light</option>
                <option value="dark">Dark</option>
                <option value="system">System</option>
              </select>
            </label>
          </div>

          {status === 'anonymous' && (
            <>
              <Button variant="ghost" onClick={openLogin}>
                {t('header.login')}
              </Button>
              <Button variant="ghost" onClick={openRegister}>
                {t('header.signup')}
              </Button>
            </>
          )}

          {status === 'authenticated' && session && (
            <>
              <span className={styles.userHome}>{session.fullName}</span>
              <Link to="/profile" className={styles.profileLink} aria-label={t('header.profile')}>
                {session.avatarUrl ? (
                  <img src={session.avatarUrl} alt="Avatar" className={styles.avatar} />
                ) : (
                  <span aria-hidden="true">{session.fullName.charAt(0).toUpperCase()}</span>
                )}
              </Link>
              <Button variant="secondary">{t('header.cart')}</Button>
              <Button variant="ghost" onClick={() => logout()}>
                {t('header.logout')}
              </Button>
            </>
          )}
        </nav>
      </Container>
    </header>
  );
}

