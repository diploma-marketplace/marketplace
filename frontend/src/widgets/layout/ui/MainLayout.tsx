import { Outlet, useMatches } from 'react-router-dom';
import { Header } from '@/widgets/header';
import { Footer } from '@/widgets/footer';
import { Sidebar } from '@/widgets/sidebar';
import { AuthModal } from '@/widgets/auth-modal';
import styles from './MainLayout.module.css';

export function MainLayout() {
  const matches = useMatches();
  const hideSidebar = matches.some(
    (match) => (match.handle as { hideSidebar?: boolean } | undefined)?.hideSidebar,
  );

  return (
    <div className={styles.layout}>
      <Header />
      <div className={[styles.body, hideSidebar ? styles.noSidebar : ''].filter(Boolean).join(' ')}>
        {!hideSidebar && <Sidebar />}
        <main className={styles.main}>
          <Outlet />
        </main>
      </div>
      <Footer />
      <AuthModal />
    </div>
  );
}
