import { Breadcrumbs, DataTable, MainCard, TableFetchParams } from '@components';
import { Chip, Grid } from '@mui/material';
import { routes } from '@routes';
import { useState } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { PagedResult } from '@services/core';
import { locales } from '@locales';

type MockUser = {
  id: number;
  name: string;
  email: string;
  role: string;
};

const mockUsers: MockUser[] = [
  { id: 1, name: 'Nguyen Van A', email: 'nguyenvana@gmail.com', role: 'Admin' },
  { id: 2, name: 'Tran Thi B', email: 'tranthib@gmail.com', role: 'User' },
  { id: 3, name: 'Le Van C', email: 'levanc@gmail.com', role: 'User' }
];

const ListUser = () => {
  const [data, setData] = useState<PagedResult<MockUser>>({
    items: [],
    pagination: {
      totalCount: mockUsers.length,
      currentPage: 1,
      pageSize: 10,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false
    }
  });

  const handleFetchData = async (params: TableFetchParams): Promise<void> => {
    const mockApi = new Promise<PagedResult<MockUser>>((resolve) => {
      setTimeout(() => {
        const currentPage = params.page + 1;
        const totalCount = mockUsers.length;
        const pageSize = params.pageSize;
        const totalPages = Math.ceil(totalCount / pageSize);
        const startIndex = params.page * pageSize;
        const endIndex = startIndex + pageSize;
        const paginatedItems = mockUsers.slice(startIndex, endIndex);

        resolve({
          items: paginatedItems,
          pagination: {
            totalCount,
            currentPage,
            pageSize,
            totalPages,
            hasNext: currentPage < totalPages,
            hasPrevious: currentPage > 1
          }
        });
      }, 300);
    });

    const result = await mockApi;
    setData(result);
  };

  const columns: ColumnDef<MockUser>[] = [
    {
      header: locales.tables.columns.id,
      accessorKey: 'id'
    },
    {
      header: locales.tables.columns.name,
      accessorKey: 'name'
    },
    {
      header: locales.tables.columns.email,
      accessorKey: 'email'
    },
    {
      header: locales.tables.columns.role,
      accessorKey: 'role',
      cell: (info: { getValue: () => unknown }) => {
        const value = (info.getValue() as string) || '';
        return <Chip label={value} color="primary" size="small" />;
      }
    }
  ];

  const breadcrumbLinks = [
    { title: locales.breadcrumbs.home, to: routes.default },
    { title: locales.breadcrumbs.users, to: routes.masterData.user.base },
    { title: locales.breadcrumbs.list }
  ];

  return (
    <>
      <Breadcrumbs heading={locales.breadcrumbs.list} links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={12}>
          <MainCard>
            <DataTable data={data.items} columns={columns} totalPage={data.pagination?.totalPages || 1} onLoad={handleFetchData} />
          </MainCard>
        </Grid>
      </Grid>
    </>
  );
};

export default ListUser;
