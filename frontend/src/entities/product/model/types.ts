export interface Product {
  id: number;
  sellerId: number;
  sellerType: 'marketplace' | 'company' | 'third-party';
  categoryId: number;
  title: string;
  description?: string;
  price: number;
  stockQuantity: number;
  createdAt: Date;
  rating?: number;
}
