import type { Product } from '../model/types';
import styles from './ProductCard.module.css';

const priceFormat = new Intl.NumberFormat('uk-UA', {
  style: 'currency',
  currency: 'UAH',
  maximumFractionDigits: 0,
});

const SELLER_LABEL: Record<Product['sellerType'], string> = {
  marketplace: 'From marketplace',
  company: 'From a company',
  'third-party': 'From a seller',
};

export function ProductCard({ product }: { product: Product }) {
  return (
    <article className={styles.card}>
      <div className={styles.image} aria-hidden="true" />
      <h3 className={styles.title}>{product.title}</h3>
      <p className={styles.price}>{priceFormat.format(product.price)}</p>
      <span className={styles.badge}>{SELLER_LABEL[product.sellerType]}</span>
    </article>
  );
}
