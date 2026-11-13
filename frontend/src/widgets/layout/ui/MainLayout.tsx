import { Outlet } from 'react-router-dom';
import { Header } from '@/widgets/header';
import { Footer } from '@/widgets/footer';
import {Sidebar} from '@/widgets/sidebar';
import styles from './MainLayout.module.css';

export function MainLayout() {
  return (
    <div className={styles.layout}>
      <Header />
      <main className={styles.main}>
        <Sidebar/>
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
