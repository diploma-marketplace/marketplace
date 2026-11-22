import { usePreferences } from '@/app/providers/usePreferences';
import styles from './SearchBar.module.css';

export function SearchBar() {
  const { t } = usePreferences();

  return (
    <form role="search" className={styles.form} onSubmit={(e) => e.preventDefault()}>
      <input
        type="search"
        className={styles.input}
        placeholder={t('search')}
        aria-label={t('searchLabel')}
      />
    </form>
  );
}