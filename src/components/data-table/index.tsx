import {
  getCoreRowModel,
  getExpandedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  PaginationState,
  useReactTable
} from '@tanstack/react-table';
import { useEffect, useState } from 'react';
import { DEFAULT_PAGE, DEFAULT_PAGE_SIZE } from '@utils/constants';
import ScrollX from '../scroll-x';
import { Table, Stack, Box, TableContainer, Divider } from '@mui/material';
import { MainCard } from '../cards';
import TablePagination from './table-pagination';
import { DataTableProps } from './types.ts';
import TableHeader from './table-header';
import TableBody from './table-body';

const DataTable = <TEntity,>({ data = [], columns, totalPage, onLoad, slots = {} }: DataTableProps<TEntity>) => {
  const [pagination, setPagination] = useState<PaginationState>({
    pageIndex: DEFAULT_PAGE,
    pageSize: DEFAULT_PAGE_SIZE
  });

  useEffect(() => {
    onLoad({
      page: pagination.pageIndex,
      pageSize: pagination.pageSize
    });
  }, [onLoad, pagination.pageIndex, pagination.pageSize]);

  const table = useReactTable({
    data,
    columns,
    state: {
      pagination
    },
    manualPagination: true,
    pageCount: totalPage,
    enableRowSelection: true,
    getFilteredRowModel: getFilteredRowModel(),
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    onPaginationChange: setPagination,
    getExpandedRowModel: getExpandedRowModel()
  });

  return (
    <MainCard content={false}>
      <ScrollX>
        <Stack>
          <TableContainer>
            <Table>
              <TableHeader headers={table.getHeaderGroups()} />

              <TableBody rows={table.getRowModel()} columnLength={columns.length} slots={slots} />
            </Table>
          </TableContainer>

          <Divider />

          <Box sx={{ p: 2 }}>
            <TablePagination
              {...{
                setPageSize: table.setPageSize,
                setPageIndex: table.setPageIndex,
                getState: table.getState,
                getPageCount: table.getPageCount
              }}
            />
          </Box>
        </Stack>
      </ScrollX>
    </MainCard>
  );
};

export default DataTable;
