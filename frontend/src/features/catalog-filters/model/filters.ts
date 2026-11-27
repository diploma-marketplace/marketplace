import type { Condition, ProductQuery, SellerType, SortKey } from '@/entities/product';

export const PAGE_SIZE = 12;

const SELLERS: SellerType[] = ['marketplace', 'company', 'fop'];
const CONDITIONS: Condition[] = ['new', 'used'];
const SORTS: SortKey[] = ['popular', 'price_asc', 'price_desc', 'new'];

const list = (p: URLSearchParams, key: string) => p.get(key)?.split(',').filter(Boolean) ?? [];

const num = (v: string | null) => {
  if (v === null || v === '') return undefined;
  const n = Number(v);
  return Number.isFinite(n) && n >= 0 ? n : undefined;
};

export function parseFilters(p: URLSearchParams): ProductQuery {
  const sort = p.get('sort') as SortKey | null;
  const page = Math.floor(Number(p.get('page')));
  return {
    q: p.get('q')?.trim() ?? '',
    category: p.get('category') ?? undefined,
    sellers: list(p, 'seller').filter((s): s is SellerType => SELLERS.includes(s as SellerType)),
    brands: list(p, 'brand'),
    conditions: list(p, 'condition').filter((c): c is Condition =>
      CONDITIONS.includes(c as Condition),
    ),
    min: num(p.get('min')),
    max: num(p.get('max')),
    sort: sort && SORTS.includes(sort) ? sort : 'popular',
    page: page >= 1 ? page : 1,
    pageSize: PAGE_SIZE,
  };
}

export function toSearchParams(f: ProductQuery): URLSearchParams {
  const p = new URLSearchParams();
  if (f.q) p.set('q', f.q);
  if (f.category) p.set('category', f.category);
  if (f.sellers.length) p.set('seller', f.sellers.join(','));
  if (f.brands.length) p.set('brand', f.brands.join(','));
  if (f.conditions.length) p.set('condition', f.conditions.join(','));
  if (f.min !== undefined) p.set('min', String(f.min));
  if (f.max !== undefined) p.set('max', String(f.max));
  if (f.sort !== 'popular') p.set('sort', f.sort);
  if (f.page > 1) p.set('page', String(f.page));
  return p;
}
