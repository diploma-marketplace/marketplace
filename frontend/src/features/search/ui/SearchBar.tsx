import styles from './SearchBar.module.css';

export function SearchBar() {
  return (
    <form role="search" className={styles.form} onSubmit={(e) => e.preventDefault()}>
      <input type="search" className={styles.input} placeholder="Search..." aria-label="Search" />
    </form>
  );
}
