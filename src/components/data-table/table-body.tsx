import { alpha, TableCell, TableRow, TableBody as MuiTableBody } from '@mui/material';
import type { Row } from '@tanstack/react-table';
import { flexRender, RowModel } from '@tanstack/react-table';
import { Fragment } from 'react/jsx-runtime';
import { locales, useTranslate } from '@locales';

type Props<TEntity> = {
  rows: RowModel<TEntity>;
  columnLength: number;
  slots?: {
    expand?: React.ReactNode | ((row: Row<TEntity>) => React.ReactNode);
  };
};

const TableBody = <TEntity,>({ rows, columnLength, slots = {} }: Props<TEntity>) => {
  const { t } = useTranslate();
  const hasExpand = typeof slots.expand !== 'undefined';

  return (
    <>
      {rows.rows.length > 0 ? (
        <MuiTableBody>
          {rows.rows.map((row) => (
            <Fragment key={row.id}>
              <TableRow sx={{ cursor: hasExpand ? 'pointer' : 'default' }} onClick={hasExpand ? row.getToggleExpandedHandler() : undefined}>
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
                  <TableCell colSpan={row.getVisibleCells().length}>
                    {typeof slots.expand === 'function' ? slots.expand(row as Row<TEntity>) : slots.expand}
                  </TableCell>
                </TableRow>
              )}
            </Fragment>
          ))}
        </MuiTableBody>
      ) : (
        <MuiTableBody>
          <TableRow>
            <TableCell colSpan={columnLength} sx={{ textAlign: 'center', p: 3 }}>
              {t(locales.tables.noData)}
            </TableCell>
          </TableRow>
        </MuiTableBody>
      )}
    </>
  );
};

export default TableBody;
