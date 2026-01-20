import { dashboardMenus, masterDataMenus } from './pages';
import { NavItemType } from './types';

export * from './types';
export * from './hook';

const menuItems: { items: NavItemType[] } = {
  items: [dashboardMenus, masterDataMenus]
};

export default menuItems;
