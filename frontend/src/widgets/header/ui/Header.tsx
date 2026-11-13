import { Link } from 'react-router-dom';
import { Container, Button } from '@/shared/ui';
import { SearchBar } from '@/features/search';
import styles from './Header.module.css';

export function Header() {
  return (
    <header className={styles.header}>
      <Container className={styles.inner}>
        <Link to="/" className={styles.logo}>
          Future Market
        </Link>
        <Button variant="secondary">Catalogue</Button>
        <SearchBar />
        <nav className={styles.actions} aria-label="actions">
          <Button variant="primary">Log In</Button>
          <Button variant="primary">Sign Up</Button>
        </nav>
      </Container>
    </header>
  );
}
