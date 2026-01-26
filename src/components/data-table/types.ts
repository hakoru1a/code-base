import type { ColumnDef, Row } from '@tanstack/react-table';
import type { ReactNode } from 'react';

export type TableFetchParams = {
  page: number;
  pageSize: number;
};

export type DataTableProps<TEntity> = {
  data: TEntity[] | undefined;
  columns: ColumnDef<TEntity>[];
  totalPage: number;
  onLoad: (params: TableFetchParams) => void;
  counts?: Record<string, number>;
  slots?: {
    expand?: ReactNode | ((row: Row<TEntity>) => ReactNode);
  };
};
