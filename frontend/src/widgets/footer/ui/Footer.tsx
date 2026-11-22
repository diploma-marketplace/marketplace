import { usePreferences } from '@/app/providers/usePreferences';
import styles from './Footer.module.css';
import { Container } from '@/shared/ui';

export function Footer() {
  const { t } = usePreferences();

  return (
    <footer className={styles.footer}>
      <Container>
        <p className={styles.text}>
          {t('footer')} {new Date().getFullYear()}
        </p>
      </Container>
    </footer>
  );
}