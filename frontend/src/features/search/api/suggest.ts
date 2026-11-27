import { mockProducts } from '@/entities/product';
import { http } from '@/shared/api/http';
import { USE_MOCKS } from '@/shared/config/env';

export async function fetchSuggestions(q: string, signal?: AbortSignal): Promise<string[]> {
  if (USE_MOCKS) {
    const text = q.toLowerCase();
    return mockProducts
      .map((p) => p.title)
      .filter((t) => t.toLowerCase().includes(text))
      .slice(0, 6);
  }
  return http<string[]>(`/search/suggest?q=${encodeURIComponent(q)}`, { signal });
}
