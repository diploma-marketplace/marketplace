export type SellerType = 'marketplace' | 'company' | 'fop';
export type Condition = 'new' | 'used';
export type SortKey = 'popular' | 'price_asc' | 'price_desc' | 'new';

export interface Product {
  id: number;
  sellerId: number;
  sellerType: SellerType;
  categoryId: number;
  title: string;
  description?: string;
  brand?: string;
  condition: Condition;
  price: number;
  stockQuantity: number;
  createdAt: string;
  rating?: number;
}

export interface ProductQuery {
  q: string;
  category?: string;
  sellers: SellerType[];
  min?: number;
  max?: number;
  brands: string[];
  conditions: Condition[];
  sort: SortKey;
  page: number;
  pageSize: number;
}

export interface ProductsPage {
  items: Product[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ProductFacets {
  brands: string[];
  conditions: Condition[];
  priceRange: { min: number; max: number };
}