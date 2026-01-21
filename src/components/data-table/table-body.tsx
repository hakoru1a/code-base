import { alpha, TableCell, TableRow, TableBody as MuiTableBody } from '@mui/material';
import { flexRender, RowModel } from '@tanstack/react-table';
import { Fragment } from 'react/jsx-runtime';
import { locales, useTranslate } from '@locales';

type Props<TEntity> = {
  rows: RowModel<TEntity>;
  columnLength: number;
  slots?: {
    expand?: React.ReactNode;
  };
};

const TableBody = <TEntity,>({ rows, columnLength, slots = {} }: Props<TEntity>) => {
  const { t } = useTranslate();

  return (
    <>
      {rows.rows.length > 0 ? (
        <MuiTableBody>
          {rows.rows.map((row) => (
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
