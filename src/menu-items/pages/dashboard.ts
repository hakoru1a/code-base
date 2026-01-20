import { NavItemType } from '../types';
import { DashboardOutlined, LineChartOutlined, ShopOutlined } from '@ant-design/icons';

const dashboardMenus: NavItemType = {
  id: 'group-dashboard',
  title: 'Dashboard',
  type: 'group',
  children: [
    {
      id: 'dashboard-page-1',
      title: 'Bảng điều khiển',
      type: 'collapse',
      url: '/dashboard',
      icon: DashboardOutlined,
      children: [
        {
          id: 'dashboard-analytics',
          title: 'Tổng quan',
          type: 'item',
          url: '/dashboard/analytics',
          icon: LineChartOutlined
        },
        {
          id: 'dashboard-sales',
          title: 'Bán hàng',
          type: 'item',
          url: '/dashboard/sales',
          icon: ShopOutlined
        }
      ]
    }
  ]
};

export default dashboardMenus;
