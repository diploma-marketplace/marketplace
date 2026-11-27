import { useState } from 'react';
import { ProductCard, useProducts, type SortKey } from '@/entities/product';
import { useCatalogFilters } from '@/features/catalog-filters';
import { Button, Pagination, Skeleton } from '@/shared/ui';
import styles from './CatalogPage.module.css';

const SORT_OPTIONS: { value: SortKey; label: string }[] = [
  { value: 'popular', label: 'Popular' },
  { value: 'price_asc', label: 'Price: low to high' },
  { value: 'price_desc', label: 'Price: high to low' },
  { value: 'new', label: 'Newest' },
];

export function CatalogPage() {
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
          {data ? `${data.total} products` : 'Loading…'}
        </p>
        <label className={styles.sort}>
          Sort
          <select
            value={filters.sort}
            onChange={(e) => update({ sort: e.target.value as SortKey })}
          >
            {SORT_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </label>
        <div className={styles.layoutSwitch} role="group" aria-label="Layout">
          <Button
            variant={layout === 'grid' ? 'primary' : 'ghost'}
            aria-pressed={layout === 'grid'}
            onClick={() => setLayout('grid')}
          >
            Grid
          </Button>
          <Button
            variant={layout === 'list' ? 'primary' : 'ghost'}
            aria-pressed={layout === 'list'}
            onClick={() => setLayout('list')}
          >
            List
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
          <p>Could not load products.</p>
          <Button variant="primary" onClick={() => refetch()}>
            Try again
          </Button>
        </div>
      )}

      {data && data.items.length === 0 && (
        <p className={styles.state}>Nothing found. Try changing the filters.</p>
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
