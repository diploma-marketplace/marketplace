import type { Product } from '../model/types';
import { usePreferences } from '@/app/providers/PreferencesProvider';
import styles from './ProductCard.module.css';

export function ProductCard({ product }: { product: Product }) {
  const { t, language } = usePreferences();

  const priceFormat = new Intl.NumberFormat(
    language === 'uk' ? 'uk-UA' : 'en-GB',
    {
      style: 'currency',
      currency: 'UAH',
      maximumFractionDigits: 0,
    }
  );

  return (
    <article className={styles.card}>
      <div className={styles.image} aria-hidden="true" />
      <h3 className={styles.title}>{product.title}</h3>
      <p className={styles.price}>{priceFormat.format(product.price)}</p>
      <span className={styles.badge}>{t(product.sellerType)}</span>
    </article>
  );
}