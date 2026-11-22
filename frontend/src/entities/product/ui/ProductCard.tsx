import type { Product, SellerType } from '../model/types';
import styles from './ProductCard.module.css';

const priceFormat = new Intl.NumberFormat('uk-UA', {
  style: 'currency',
  currency: 'UAH',
  maximumFractionDigits: 0,
});

const SELLER_LABEL: Record<SellerType, string> = {
  marketplace: 'From marketplace',
  company: 'From a company',
  fop: 'From a private seller',
};

interface ProductCardProps {
  product: Product;
  layout?: 'grid' | 'list';
}

export function ProductCard({ product, layout = 'grid' }: ProductCardProps) {
  const cls = layout === 'list' ? `${styles.card} ${styles.list}` : styles.card;
  return (
    <article className={cls}>
      <div className={styles.image} aria-hidden="true" />
      <div className={styles.info}>
        <h3 className={styles.title}>{product.title}</h3>
        <p className={styles.meta}>
          {[product.brand, product.condition === 'used' ? 'Used' : 'New'].filter(Boolean).join(' · ')}
        </p>
        <p className={styles.price}>{priceFormat.format(product.price)}</p>
        <span className={styles.badge}>{SELLER_LABEL[product.sellerType]}</span>
      </div>
    </article>
  );
}