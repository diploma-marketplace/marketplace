import type { Category } from '../model/types';
import styles from './CategoryCard.module.css';

export function CategoryCard({ category }: { category: Category }) {
  return (
    <article className={styles.card}>
        <h3 className={styles.name}>{category.name}</h3>
    </article>
  );
}
