import { usePreferences } from '@/app/providers/usePreferences';
import styles from './Sidebar.module.css';

export function Sidebar() {
  const { t } = usePreferences();

  return <aside className={styles.sidebar} aria-label={t('categories')} />;
}