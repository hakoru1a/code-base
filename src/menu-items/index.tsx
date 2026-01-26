import { dashboardMenus, contractsMenus, factoryMenus, masterDataMenus } from './pages';
import { NavItemType } from './types';

export * from './types';
export * from './hook';

const menuItems: { items: NavItemType[] } = {
  items: [dashboardMenus, contractsMenus, factoryMenus, masterDataMenus]
};

export default menuItems;
