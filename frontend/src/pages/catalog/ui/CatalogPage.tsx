import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ProductCard, useProducts, type SortKey } from '@/entities/product';
import { useCatalogFilters } from '@/features/catalog-filters';
import { Button, Pagination, Skeleton } from '@/shared/ui';
import styles from './CatalogPage.module.css';

const SORT_OPTIONS: { value: SortKey; label: string }[] = [
  { value: 'popular', label: 'catalog.popular' },
  { value: 'price_asc', label: 'catalog.priceLowToHigh' },
  { value: 'price_desc', label: 'catalog.priceHighToLow' },
  { value: 'new', label: 'catalog.newest' },
];

export function CatalogPage() {
  const { t } = useTranslation();
  const { filters, update } = useCatalogFilters();
  const [layout, setLayout] = useState<'grid' | 'list'>('grid');
  const { data, isPending, isError, isPlaceholderData, refetch } = useProducts(filters);

  const pageCount = data ? Math.max(1, Math.ceil(data.total / data.pageSize)) : 0;
  const gridClass = layout === 'grid' ? styles.grid : styles.list;

  const changePage = (page: number) => {
    update({ page });
    window.scrollTo({ top: 0 });
  };

  return (
    <div className={styles.page}>
      <h1 className="visually-hidden">Catalog</h1>

      <div className={styles.toolbar}>
        <p className={styles.count} aria-live="polite">
          {data ? t('catalog.productsCount', { count: data.total }) : t('catalog.loading')}
        </p>
        <label className={styles.sort}>
          {t('catalog.sort')}
          <select
            value={filters.sort}
            onChange={(e) => update({ sort: e.target.value as SortKey })}
          >
            {SORT_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {t(o.label)}
              </option>
            ))}
          </select>
        </label>
        <div className={styles.layoutSwitch} role="group" aria-label={t('catalog.layout')}>
          <Button
            variant={layout === 'grid' ? 'primary' : 'ghost'}
            aria-pressed={layout === 'grid'}
            onClick={() => setLayout('grid')}
          >
            {t('catalog.grid')}
          </Button>
          <Button
            variant={layout === 'list' ? 'primary' : 'ghost'}
            aria-pressed={layout === 'list'}
            onClick={() => setLayout('list')}
          >
            {t('catalog.list')}
          </Button>
        </div>
      </div>

      {isPending && (
        <div className={gridClass} aria-busy="true">
          {Array.from({ length: 8 }, (_, i) => (
            <Skeleton key={i} height={layout === 'grid' ? 240 : 120} />
          ))}
        </div>
      )}

      {isError && (
        <div role="alert" className={styles.state}>
          <p>{t('catalog.loadError')}</p>
          <Button variant="primary" onClick={() => refetch()}>
            {t('catalog.tryAgain')}
          </Button>
        </div>
      )}

      {data && data.items.length === 0 && (
        <p className={styles.state}>{t('catalog.nothingFound')}</p>
      )}

      {data && data.items.length > 0 && (
        <div className={`${gridClass} ${isPlaceholderData ? styles.stale : ''}`}>
          {data.items.map((p) => (
            <ProductCard key={p.id} product={p} layout={layout} />
          ))}
        </div>
      )}

      <Pagination page={filters.page} pageCount={pageCount} onChange={changePage} />
    </div>
  );
}