import { USE_MOCKS } from '@/shared/config/env';
import { http } from '@/shared/api/http';
import { mockFetchFacets, mockFetchProducts } from './mockApi';
import type { ProductFacets, ProductQuery, ProductsPage } from '../model/types';

function toApiParams(q: ProductQuery): URLSearchParams {
  const p = new URLSearchParams();
  if (q.q) p.set('q', q.q);
  if (q.category) p.set('category', q.category);
  if (q.sellers.length) p.set('seller', q.sellers.join(','));
  if (q.brands.length) p.set('brand', q.brands.join(','));
  if (q.conditions.length) p.set('condition', q.conditions.join(','));
  if (q.min !== undefined) p.set('min', String(q.min));
  if (q.max !== undefined) p.set('max', String(q.max));
  p.set('sort', q.sort);
  p.set('page', String(q.page));
  p.set('pageSize', String(q.pageSize));
  return p;
}

export function fetchProducts(q: ProductQuery, signal?: AbortSignal): Promise<ProductsPage> {
  if (USE_MOCKS) return mockFetchProducts(q, signal);
  return http<ProductsPage>(`/products?${toApiParams(q)}`, { signal });
}

export function fetchFacets(category?: string): Promise<ProductFacets> {
  if (USE_MOCKS) return mockFetchFacets(category);
  return http<ProductFacets>(`/categories/${category ?? 'all'}/facets`);
}
