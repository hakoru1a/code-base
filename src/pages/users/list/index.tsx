import { Breadcrumbs, DataTable, MainCard, TableFetchParams } from '@components';
import { Chip, Grid } from '@mui/material';
import { routes } from '@routes';
import { useState } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { PaginationResult } from '@services/core';

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
  const [data, setData] = useState<PaginationResult<MockUser>>({
    data: [],
    meta: {
      page: 1,
      pageSize: 10,
      totalPages: 1,
      size: mockUsers.length,
      total: mockUsers.length,
      canNext: false,
      canPrevious: false
    }
  });

  const handleFetchData = async (params: TableFetchParams) => {
    const mockApi = new Promise((resolve) => {
      setTimeout(() => {
        resolve({
          data: mockUsers,
          meta: {
            page: params.page + 1,
            pageSize: params.pageSize,
            totalPages: 1,
            size: mockUsers.length,
            total: mockUsers.length,
            canNext: false,
            canPrevious: false
          }
        });
      });
    });
    const data = await mockApi;
    setData(data as PaginationResult<MockUser>);
  };

  const breadcrumbLinks = [
    { title: 'Trang chủ', to: routes.default },
    { title: 'Users', to: routes.masterData.user.base },
    { title: 'Danh sách' }
  ];

  const columns: ColumnDef<MockUser>[] = [
    {
      header: 'ID',
      accessorKey: 'id'
    },
    {
      header: 'Name',
      accessorKey: 'name'
    },
    {
      header: 'Email',
      accessorKey: 'email'
    },
    {
      header: 'Role',
      accessorKey: 'role',
      cell: (cell) => {
        const value = cell.getValue();
        return <Chip label={<>{value || ''}</>} color="primary" size="small" />;
      }
    }
  ];

  return (
    <>
      <Breadcrumbs custom heading="Danh sách" links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={12}>
          <MainCard>
            <DataTable data={data.data} columns={columns} totalPage={data.meta!.totalPages} onLoad={handleFetchData} />
          </MainCard>
        </Grid>
      </Grid>
    </>
  );
};

export default ListUser;
