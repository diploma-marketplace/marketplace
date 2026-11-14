import type {Product} from './types';

const sellerTypes: Product['sellerType'][] = ['marketplace', 'company', 'third-party'];

export const mockProducts: Product[] = Array.from({ length: 8}, (_, i) => ({
    id: i+1,
    sellerId: 1,
    sellerType: sellerTypes[i % sellerTypes.length],
    categoryId: (i % 7) + 1,
    title: `Product ${i +1}`,
    price: 1000 + i * 350,
    stockQuantity: 10,
    createdAt: new Date('2026-01-01'),
    rating: 4 + (i%2) * 0.5,
}));