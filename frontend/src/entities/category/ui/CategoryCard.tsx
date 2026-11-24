import type { Category } from '../model/types';
import styles from './CategoryCard.module.css';
import { useTranslation } from 'react-i18next';

const categoryTranslations: Record<string, string> = {
    electronics: 'catalog.electronics',
    appliances: 'catalog.appliances',
    fashion: 'catalog.fashion',
    home_garden: 'catalog.homeGarden',
    sports: 'catalog.sports',
    kids: 'catalog.kids',
    auto: 'catalog.auto',
};
export function CategoryCard({ category }: { category: Category }) {
  const { t } = useTranslation(); 
  return (
    <article className={styles.card}>
      <h3 className={styles.name}>
        {categoryTranslations[category.slug]
          ? t(categoryTranslations[category.slug])
          : category.name}
      </h3>
    </article>
  );
}
