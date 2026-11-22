import { useCallback, useMemo } from 'react';
import { useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import type { ProductQuery } from '@/entities/product';
import { parseFilters, toSearchParams } from './filters';

export function useCatalogFilters() {
  const [params] = useSearchParams();
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const filters = useMemo(() => parseFilters(params), [params]);

  const apply = useCallback(
    (next: ProductQuery) => {
      const qs = toSearchParams(next).toString();
      navigate(
        { pathname: '/catalog', search: qs ? `?${qs}` : '' },
        { replace: pathname === '/catalog' },
      );
    },
    [navigate, pathname],
  );

  const update = useCallback(
    (patch: Partial<ProductQuery>) => {
      const next = { ...filters, ...patch };
      // будь-яка зміна фільтрів скидає сторінку; зміна категорії скидає атрибути категорії
      if (!('page' in patch)) next.page = 1;
      if ('category' in patch && patch.category !== filters.category) {
        next.brands = [];
        next.conditions = [];
        next.min = undefined;
        next.max = undefined;
      }
      apply(next);
    },
    [filters, apply],
  );

  const reset = useCallback(
    () => apply({ ...parseFilters(new URLSearchParams()), q: filters.q }),
    [filters.q, apply],
  );

  return { filters, update, reset };
}
