import styles from './Footer.module.css';
import { Container } from '@/shared/ui';

export function Footer() {
  return (
    <footer className={styles.footer}>
      <Container>
        <p className={styles.text}>© {new Date().getFullYear()} Future Market</p>
      </Container>
    </footer>
  );
}
