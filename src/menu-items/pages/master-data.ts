import { NavItemType } from '../types';
import { routes } from '@routes';

const masterDataMenus: NavItemType = {
  id: 'dashboard',
  title: 'Dashboard',
  type: 'group',
  children: [
    {
      id: 'master-data-users',
      title: 'Users',
      type: 'item',
      url: routes.masterData.user.base
    }
  ]
};

export default masterDataMenus;
