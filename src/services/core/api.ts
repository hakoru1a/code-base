export type ApiResult<TData> = {
  success: boolean;
  status: number;
  data?: TData;
  errors?: string[];
  metaData?: DynamicObject;
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

export type PaginationMetadata = {
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
};

export type PagedResult<TData> = {
  items: TData[];
  pagination?: PaginationMetadata;
};
