import { CategoryCard, mockCategories } from '@/entities/category';
import styles from './sections.module.css';

export function CategorySection() {
  return (
    <section aria-labelledby="home-categories-title">
      <h2 id="home-categories-title" className="visually-hidden">
        Categories
      </h2>
      <div className={styles.categoryGrid}>
        {mockCategories.map((category) => (
          <CategoryCard category={category} key={category.id} />
        ))}
      </div>
    </section>
  );
}
