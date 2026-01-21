import { NavItemType } from '../types';
import { routes } from '@routes';
import { locales } from '@locales';

const masterDataMenus: NavItemType = {
  id: 'master-data',
  title: locales.menus.masterData.title,
  type: 'group',
  children: [
    {
      id: 'master-data-users',
      title: locales.menus.masterData.subMenus.users,
      type: 'item',
      url: routes.masterData.user.base
    }
  ]
};

export default masterDataMenus;
