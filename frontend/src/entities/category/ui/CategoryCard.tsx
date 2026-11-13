import type { Category } from '../model/types';
import styles from './CategoryCard.module.css';

export function CategoryCard({ category }: { category: Category }) {
  return (
    <article className={styles.card}>
      <div className={styles.image} aria-hidden="true">
        <h4 className={styles.name}>{category.name}</h4>
      </div>
    </article>
  );
}
