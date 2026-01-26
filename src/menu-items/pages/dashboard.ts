import { NavItemType } from '../types';
import { DashboardOutlined, LineChartOutlined } from '@ant-design/icons';
import { locales } from '@locales';
import { routes } from '@routes';

const dashboardMenus: NavItemType = {
  id: 'group-dashboard',
  title: locales.menus.dashboard.title,
  type: 'group',
  children: [
    {
      id: 'dashboard-page-collapse',
      title: locales.menus.dashboard.title,
      type: 'collapse',
      url: routes.dashboard.base,
      icon: DashboardOutlined,
      children: [
        {
          id: 'dashboard-analytics',
          title: locales.menus.dashboard.subMenus.overview,
          type: 'item',
          url: routes.dashboard.overview,
          icon: LineChartOutlined
        }
      ]
    }
  ]
};

export default dashboardMenus;
