import type { Category } from '../model/types';
import { usePreferences } from '@/app/providers/usePreferences';
import styles from './CategoryCard.module.css';

export function CategoryCard({ category }: { category: Category }) {
  const { t } = usePreferences();

  return (
    <article className={styles.card}>
      <h3 className={styles.name}>{t(category.slug)}</h3>
    </article>
  );
}