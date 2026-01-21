import {
  flexRender,
  getCoreRowModel,
  getExpandedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  HeaderGroup,
  PaginationState,
  useReactTable
} from '@tanstack/react-table';
import { Fragment, useEffect, useState } from 'react';
import { DEFAULT_PAGE, DEFAULT_PAGE_SIZE } from '@utils/constants';
import ScrollX from '../scroll-x';
import { Table, TableHead, TableCell, TableRow, Stack, Box, TableContainer, TableBody, Divider, alpha } from '@mui/material';
import { MainCard } from '../cards';
import HeaderSort from './header-sort';
import TablePagination from './table-pagination';
import { DataTableProps } from './types.ts';

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
              <TableHead>
                {table.getHeaderGroups().map((headerGroup: HeaderGroup<Dynamic>) => (
                  <TableRow key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <TableCell
                        key={header.id}
                        {...header.column.columnDef.meta}
                        onClick={header.column.getToggleSortingHandler()}
                        {...(header.column.getCanSort() &&
                          !!header.column.columnDef.meta && {
                            className: 'cursor-pointer prevent-select'
                          })}
                      >
                        {header.isPlaceholder ? null : (
                          <Stack direction="row" sx={{ gap: 1, alignItems: 'center', justifyContent: 'space-between' }}>
                            <Box>{flexRender(header.column.columnDef.header, header.getContext())}</Box>
                            {header.column.getCanSort() && <HeaderSort column={header.column} />}
                          </Stack>
                        )}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableHead>

              {table.getRowModel().rows.length > 0 ? (
                <TableBody>
                  {table.getRowModel().rows.map((row) => (
                    <Fragment key={row.id}>
                      <TableRow sx={{ cursor: 'pointer' }}>
                        {row.getVisibleCells().map((cell) => (
                          <TableCell key={cell.id} {...cell.column.columnDef.meta}>
                            {flexRender(cell.column.columnDef.cell, cell.getContext())}
                          </TableCell>
                        ))}
                      </TableRow>
                      {row.getIsExpanded() && (
                        <TableRow
                          sx={(theme) => ({
                            bgcolor: alpha(theme.palette.primary.lighter, 0.1),
                            '&:hover': { bgcolor: `${alpha(theme.palette.primary.lighter, 0.1)} !important` }
                          })}
                        >
                          <TableCell colSpan={row.getVisibleCells().length}>{slots?.expand}</TableCell>
                        </TableRow>
                      )}
                    </Fragment>
                  ))}
                </TableBody>
              ) : (
                <TableBody>
                  <TableRow>
                    <TableCell colSpan={columns.length} sx={{ textAlign: 'center', p: 3 }}>
                      Không có dữ liệu
                    </TableCell>
                  </TableRow>
                </TableBody>
              )}
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
