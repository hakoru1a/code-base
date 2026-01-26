import { NavItemType } from '../types';
import { DashboardOutlined, LineChartOutlined, ShopOutlined, ProfileOutlined } from '@ant-design/icons';
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
        },
        {
          id: 'dashboard-sales',
          title: locales.menus.dashboard.subMenus.sale,
          type: 'item',
          url: routes.dashboard.sales,
          icon: ShopOutlined
        },
        {
          id: 'dashboard-business-plans',
          title: locales.menus.dashboard.subMenus.businessPlans,
          type: 'item',
          url: routes.dashboard.businessPlans,
          icon: ProfileOutlined
        }
      ]
    }
  ]
};

export default dashboardMenus;
