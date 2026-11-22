import { ProductCard, mockProducts } from '@/entities/product';
import { usePreferences } from '@/app/providers/PreferencesProvider';
import styles from './sections.module.css';

export function RecommendedSection() {
  const { t } = usePreferences();

  return (
    <section className={styles.products} aria-labelledby="home-products-title">
      <h2 className="visually-hidden" id="home-products-title">
        {t('recommended')}
      </h2>
      <div className={styles.productGrid}>
        {mockProducts.map((product) => (
          <ProductCard product={product} key={product.id} />
        ))}
      </div>
    </section>
  );
}