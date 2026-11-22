import { CategoryCard, mockCategories } from '@/entities/category';
import { usePreferences } from '@/app/providers/usePreferences';
import styles from './sections.module.css';

export function CategorySection() {
  const { t } = usePreferences();

  return (
    <section aria-labelledby="home-categories-title">
      <h2 id="home-categories-title" className="visually-hidden">
        {t('categories')}
      </h2>
      <div className={styles.categoryGrid}>
        {mockCategories.map((category) => (
          <CategoryCard category={category} key={category.id} />
        ))}
      </div>
    </section>
  );
}