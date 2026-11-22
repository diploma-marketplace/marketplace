import { Outlet } from 'react-router-dom';
import { Header } from '@/widgets/header';
import { Footer } from '@/widgets/footer';
import { AuthModal } from '@/widgets/auth-modal';
import styles from './MainLayout.module.css';

export function MainLayout() {
    return (
        <div className={styles.layout}>
            <Header />
            <div className={styles.content}>
                <Outlet />
            </div>
            <Footer />
            <AuthModal />
        </div>
    );
}