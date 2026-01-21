import { flexRender, HeaderGroup } from '@tanstack/react-table';
import { Box, Stack, TableCell, TableHead, TableRow } from '@mui/material';
import HeaderSort from './header-sort.tsx';
import { useTranslate } from '@locales';

type Props<TEntity> = {
  headers: HeaderGroup<TEntity>[];
};

const TableHeader = <TEntity,>({ headers }: Props<TEntity>) => {
  const { t } = useTranslate();

  return (
    <TableHead>
      {headers.map((headerGroup: HeaderGroup<Dynamic>) => (
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
                  <Box>{flexRender(t(header.column.columnDef.header as string), header.getContext())}</Box>
                  {header.column.getCanSort() && <HeaderSort column={header.column} />}
                </Stack>
              )}
            </TableCell>
          ))}
        </TableRow>
      ))}
    </TableHead>
  );
};

export default TableHeader;
