import { ColumnDef } from '@tanstack/react-table';
import { ReactNode } from 'react';

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
    expand?: ReactNode;
  };
};
