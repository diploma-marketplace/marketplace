import { mockCategories } from '@/entities/category';
import { mockProducts } from '../model/mocks';
import type { Product, ProductFacets, ProductQuery, ProductsPage } from '../model/types';

function delay(ms: number, signal?: AbortSignal) {
  return new Promise<void>((resolve, reject) => {
    const id = window.setTimeout(resolve, ms);
    signal?.addEventListener('abort', () => {
      window.clearTimeout(id);
      reject(signal.reason);
    });
  });
}

const inCategory = (p: Product, slug?: string) => {
  if (!slug) return true;
  const cat = mockCategories.find((c) => c.slug === slug);
  return !cat || p.categoryId === cat.id;
};

export async function mockFetchProducts(q: ProductQuery, signal?: AbortSignal): Promise<ProductsPage> {
  await delay(300, signal);
  const text = q.q.toLowerCase();
  const items = mockProducts.filter(
    (p) =>
      inCategory(p, q.category) &&
      (!text || p.title.toLowerCase().includes(text)) &&
      (q.sellers.length === 0 || q.sellers.includes(p.sellerType)) &&
      (q.brands.length === 0 || (p.brand !== undefined && q.brands.includes(p.brand))) &&
      (q.conditions.length === 0 || q.conditions.includes(p.condition)) &&
      (q.min === undefined || p.price >= q.min) &&
      (q.max === undefined || p.price <= q.max),
  );
  items.sort((a, b) => {
    switch (q.sort) {
      case 'price_asc':
        return a.price - b.price;
      case 'price_desc':
        return b.price - a.price;
      case 'new':
        return b.id - a.id;
      default:
        return (b.rating ?? 0) - (a.rating ?? 0);
    }
  });
  const start = (q.page - 1) * q.pageSize;
  return { items: items.slice(start, start + q.pageSize), total: items.length, page: q.page, pageSize: q.pageSize };
}

export async function mockFetchFacets(category?: string): Promise<ProductFacets> {
  await delay(150);
  const items = mockProducts.filter((p) => inCategory(p, category));
  const prices = items.map((p) => p.price);
  return {
    brands: [...new Set(items.map((p) => p.brand).filter((b): b is string => !!b))].sort(),
    conditions: [...new Set(items.map((p) => p.condition))],
    priceRange: { min: Math.min(...prices), max: Math.max(...prices) },
  };
}