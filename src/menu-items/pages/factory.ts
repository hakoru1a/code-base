import { NavItemType } from '../types';
import { ProfileOutlined, ShopOutlined } from '@ant-design/icons';
import { locales } from '@locales';
import { routes } from '@routes';

const factoryMenus: NavItemType = {
  id: 'group-factory',
  title: locales.menus.factory.title,
  type: 'group',
  children: [
    {
      id: 'factory-page-collapse',
      title: locales.menus.factory.title,
      type: 'collapse',
      url: routes.dashboard.base,
      icon: ProfileOutlined,
      children: [
        {
          id: 'factory-materials-collapse',
          title: locales.menus.factory.subMenus.materials,
          type: 'collapse',
          url: routes.dashboard.base,
          icon: ShopOutlined,
          children: [
            {
              id: 'factory-weigh-tickets',
              title: locales.menus.factory.subMenus.weighTickets,
              type: 'item',
              url: routes.dashboard.weighTickets
            },
            {
              id: 'factory-production-assignments-list',
              title: locales.menus.factory.subMenus.productionAssignments,
              type: 'item',
              url: routes.dashboard.productionAssignments.list
            },
            {
              id: 'factory-production-assignments-vehicles',
              title: locales.menus.factory.subMenus.productionAssignVehicles,
              type: 'item',
              url: routes.dashboard.productionAssignments.vehicles
            },
            {
              id: 'factory-production-assignments-staff',
              title: locales.menus.factory.subMenus.productionAssignStaff,
              type: 'item',
              url: routes.dashboard.productionAssignments.staff
            }
          ]
        }
      ]
    }
  ]
};

export default factoryMenus;
