export type ApiResult<TData> = {
  success: boolean;
  status: number;
  data?: TData;
  error?: string;
};

export enum ApiVersion {
  v1 = '/api/v1'
}

export type Entity<TId> = {
  id: TId;
};

export type EntityWithName<TId> = Entity<TId> & {
  name: string;
};

export type PaginationRequest = {
  page?: number;
  size?: number;
};

export type SearchRequest = {
  search?: string;
};

export type SortDirection = 'asc' | 'desc';

export type SortRequest = {
  sortBy?: string;
  sortDirection?: SortDirection;
};

export type QueryRequest = PaginationRequest & SearchRequest & SortRequest;

export type PagingResponse = {
  canPrevious: boolean;
  canNext: boolean;
  page: number;
  size: number;
  total: number;
  totalPages: number;
};

export type PaginationResult<TData> = {
  data: TData[];
  meta?: PagingResponse & DynamicObject;
};
