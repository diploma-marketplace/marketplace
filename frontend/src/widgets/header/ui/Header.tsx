import { Link } from 'react-router-dom';
import { Container, Button } from '@/shared/ui';
import { SearchBar } from '@/features/search';
import styles from './Header.module.css';
import { useAuth, useAuthModal } from '@/features/auth';

export function Header() {
  const { status, session, logout } = useAuth();
  const { openLogin, openRegister } = useAuthModal();

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
          {status === 'anonymous' && (
            <>
              <Button variant="ghost" onClick={openLogin}>
                Log In
              </Button>
              <Button variant="ghost" onClick={openRegister}>
                Sign up
              </Button>
            </>
          )}
          {status === 'authenticated' && session && (
            <>
              <span className={styles.userHome}>{session.fullName}</span>
              <Link to="/profile" className={styles.profileLink} aria-label="Profile">
                {session.avatarUrl ? (
                  <img src={session.avatarUrl} alt="Avatar" className={styles.avatar} />
                ) : (
                  <span aria-hidden="true">{session.fullName.charAt(0).toUpperCase()}</span>
                )}
              </Link>
              <Button variant="secondary">Cart</Button>
              <Button variant="ghost" onClick={() => logout()}>
                Logout
              </Button>
            </>
          )}
        </nav>
      </Container>
    </header>
  );
}
