import { useFacets } from '@/entities/product';
import { mockCategories } from '@/entities/category';
import { Button } from '@/shared/ui';
import { useCatalogFilters } from '../model/useCatalogFilters';
import { SellerFilter } from './SellerFilter';
import { PriceRange } from './PriceRange';
import styles from './Filters.module.css';

const toggleIn = <T,>(list: T[], item: T) =>
  list.includes(item) ? list.filter((i) => i !== item) : [...list, item];

export function FiltersPanel() {
  const { filters, update, reset } = useCatalogFilters();
  const { data: facets } = useFacets(filters.category);

  return (
    <div className={styles.panel}>
      <SellerFilter value={filters.sellers} onChange={(sellers) => update({ sellers })} />

      <fieldset className={styles.group}>
        <legend className={styles.legend}>Category</legend>
        <label className={styles.check}>
          <input
            type="radio"
            name="category"
            checked={!filters.category}
            onChange={() => update({ category: undefined })}
          />
          All categories
        </label>
        {mockCategories.map((c) => (
          <label key={c.id} className={styles.check}>
            <input
              type="radio"
              name="category"
              checked={filters.category === c.slug}
              onChange={() => update({ category: c.slug })}
            />
            {c.name}
          </label>
        ))}
      </fieldset>

      {facets && (
        <PriceRange
          key={`${facets.priceRange.min}-${facets.priceRange.max}-${filters.min}-${filters.max}`}
          bounds={facets.priceRange}
          min={filters.min}
          max={filters.max}
          onCommit={(min, max) => update({ min, max })}
        />
      )}

      {facets && facets.brands.length > 0 && (
        <fieldset className={styles.group}>
          <legend className={styles.legend}>Brand</legend>
          {facets.brands.map((b) => (
            <label key={b} className={styles.check}>
              <input
                type="checkbox"
                checked={filters.brands.includes(b)}
                onChange={() => update({ brands: toggleIn(filters.brands, b) })}
              />
              {b}
            </label>
          ))}
        </fieldset>
      )}

      {facets && facets.conditions.length > 0 && (
        <fieldset className={styles.group}>
          <legend className={styles.legend}>Condition</legend>
          {facets.conditions.map((c) => (
            <label key={c} className={styles.check}>
              <input
                type="checkbox"
                checked={filters.conditions.includes(c)}
                onChange={() => update({ conditions: toggleIn(filters.conditions, c) })}
              />
              {c === 'new' ? 'New' : 'Used'}
            </label>
          ))}
        </fieldset>
      )}

      <Button variant="ghost" onClick={reset}>
        Reset filters
      </Button>
    </div>
  );
}