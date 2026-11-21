import { Link } from 'react-router-dom';
import { Container, Button } from '@/shared/ui';
import { SearchBar } from '@/features/search';
import styles from './Header.module.css';
import { useAuth, useAuthModal} from '@/features/auth';

export function Header() {
  const {status, session, logout} = useAuth();
  const {openLogin, openRegister} = useAuthModal();

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
          {status === 'anonymous' && (
              <>
                <Button variant="ghost" onClick={openLogin}>Log In</Button>
                <Button variant="ghost" onClick={openRegister}>Sign up</Button>
              </>
          )}
          {status === 'authenticated' && session && (
              <>
                <span className={styles.userHome}>{session.fullName}</span>
                <Button variant="ghost" onClick={() => void logout()}></Button>
              </>
          )}
        </nav>
      </Container>
    </header>
  );
}
