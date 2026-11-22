import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { fetchFacets, fetchProducts } from './productApi';
import type { ProductQuery } from '../model/types';

export function useProducts(query: ProductQuery) {
  return useQuery({
    queryKey: ['products', query],
    queryFn: ({ signal }) => fetchProducts(query, signal),
    placeholderData: keepPreviousData,
  });
}

export function useFacets(category?: string) {
  return useQuery({
    queryKey: ['facets', category ?? 'all'],
    queryFn: () => fetchFacets(category),
    placeholderData: keepPreviousData,
  });
}