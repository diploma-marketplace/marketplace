import { API_URL } from '@/shared/config/env';

interface ApiErrorBody {
  status?: number;
  code?: string;
  message?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  status: number;
  code: string;
  fieldErrors: Record<string, string[]>;

  constructor(
    status: number,
    code: string,
    message: string,
    fieldErrors: Record<string, string[]> = {},
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.fieldErrors = fieldErrors;
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  body?: unknown;
  signal?: AbortSignal;
}

function parseJson(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return undefined;
  }
}

function toApiError(status: number, data: unknown): ApiError {
  const body = (typeof data === 'object' && data !== null ? data : {}) as ApiErrorBody;
  const fallback =
    status >= 500 ? 'Something went wrong on our side. Please try again later.' : 'Request failed.';

  return new ApiError(
    status,
    body.code ?? 'UNKNOWN_ERROR',
    status >= 500 ? fallback : (body.message ?? fallback),
    body.errors ?? {},
  );
}

export async function request<T>(
  path: string,
  { method = 'GET', body, signal }: RequestOptions = {},
): Promise<T> {
  const hasBody = body !== undefined;

  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method,
      credentials: 'include',
      signal,
      headers: {
        Accept: 'application/json',
        ...(hasBody ? { 'Content-Type': 'application/json' } : {}),
      },
      body: hasBody ? JSON.stringify(body) : undefined,
    });
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw error;
    throw new ApiError(
      0,
      'NETWORK_ERROR',
      'Cannot reach the server. Check your connection and try again.',
    );
  }

  const text = await response.text();
  const data = text ? parseJson(text) : undefined;

  if (!response.ok) throw toApiError(response.status, data);

  return data as T;
}
