import { Link } from 'react-router-dom';
import { Container, Button } from '@/shared/ui';
import { SearchBar } from '@/features/search';
import styles from './Header.module.css';

export function Header() {
  return (
    <header className={styles.header}>
      <Container className={styles.inner}>
        <Link to="/" className={styles.logo} aria-label="FutureMarket, home page">
          <span className={styles.logoMark} aria-hidden="true"/>
          <span className={styles.logo}>Future Market</span>
        </Link>
        <div className={styles.search}>
          <SearchBar />
        </div>
        <nav className={styles.actions} aria-label="actions">
          <Button variant="ghost">Log In</Button>
          <Button variant="ghost">Sign Up</Button>
        </nav>
      </Container>
    </header>
  );
}
