import type { Product, SellerType } from './types';

const sellerTypes: SellerType[] = ['marketplace', 'company', 'fop'];
const brands = ['Samsung', 'Apple', 'Bosch', 'Nike', 'Xiaomi'];

export const mockProducts: Product[] = Array.from({ length: 40 }, (_, i) => ({
  id: i + 1,
  sellerId: 1,
  sellerType: sellerTypes[i % sellerTypes.length],
  categoryId: (i % 7) + 1,
  title: `Product ${i + 1}`,
  brand: brands[i % brands.length],
  condition: i % 4 === 0 ? 'used' : 'new',
  price: 1000 + i * 350,
  stockQuantity: 10,
  createdAt: '2026-01-01T00:00:00Z',
  rating: 4 + (i % 2) * 0.5,
}));
