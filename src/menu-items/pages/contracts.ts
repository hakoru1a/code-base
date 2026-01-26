import { NavItemType } from '../types';
import { ShopOutlined, ProfileOutlined } from '@ant-design/icons';
import { locales } from '@locales';
import { routes } from '@routes';

const contractsMenus: NavItemType = {
  id: 'group-contracts',
  title: locales.menus.contracts.title,
  type: 'group',
  children: [
    {
      id: 'contracts-page-collapse',
      title: locales.menus.contracts.title,
      type: 'collapse',
      url: routes.dashboard.base,
      icon: ProfileOutlined,
      children: [
        {
          id: 'contracts-sales',
          title: locales.menus.contracts.subMenus.sale,
          type: 'item',
          url: routes.dashboard.sales,
          icon: ShopOutlined
        },
        {
          id: 'contracts-business-plans',
          title: locales.menus.contracts.subMenus.businessPlans,
          type: 'item',
          url: routes.dashboard.businessPlans,
          icon: ProfileOutlined
        }
      ]
    }
  ]
};

export default contractsMenus;
