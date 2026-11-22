import { Outlet } from 'react-router-dom';
import { Sidebar } from '@/widgets/sidebar';
import styles from './SidebarLayout.module.css';

export function SidebarLayout() {
  return (
    <div className={styles.body}>
      <Sidebar />
      <main className={styles.main}>
        <Outlet />
      </main>
    </div>
  );
}
