import { I18n } from '@locales';
import type { FontFamily, PresetColor } from '@themes';

export type CustomizationProps = {
  fontFamily: FontFamily;
  locale: I18n;
  miniDrawer: boolean;
  container: boolean;
  menuOrientation: MenuOrientation;
  mode: ThemeMode;
  presetColor: PresetColor;
  themeDirection: ThemeDirection;
  onChangeContainer: (container: boolean) => void;
  onChangeLocalization: (lang: I18n) => void;
  onChangeMode: (mode: ThemeMode) => void;
  onChangePresetColor: (theme: PresetColor) => void;
  onChangeDirection: (direction: ThemeDirection) => void;
  onChangeMiniDrawer: (miniDrawer: boolean) => void;
  onChangeThemeLayout: (direction: ThemeDirection, miniDrawer: boolean) => void;
  onChangeMenuOrientation: (menuOrientation: MenuOrientation) => void;
  onChangeFontFamily: (fontFamily: FontFamily) => void;
};

export interface ConfigData {
  id?: number;
  key: string;
  value: string;
  description?: string;
}

export interface CodeDetailData {
  id?: number;
  codeId: number;
  name: string;
  description: string;
  value: string;
  lastUpdatedAt?: string;
}

export interface ConfigMetaData {
  screenNames: string[];
  [key: string]: Dynamic; // Cho phép thêm các trường khác trong tương lai
}

export interface Config {
  id: number;
  code: string;
  name: string;
  description: string;
  codeType: string;
  metaData: ConfigMetaData;
  data: ConfigData[];
  status: number; // 1: active, 0: inactive
  createdAt?: string;
  updatedAt?: string;
  lastUpdatedAt?: string;
}

export interface ConfigFormData {
  code: string;
  name: string;
  description: string;
  codeType: string;
  metaData: ConfigMetaData;
  data: ConfigData[];
}

export interface ConfigParams {
  page?: number;
  limit?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  code?: string;
  name?: string;
  codeType?: string;
  screenName?: string;
  status?: number;
}

export interface ConfigListResponse {
  data: Config[];
  meta: {
    message: string;
  };
}

export interface SingleConfigResponse {
  data: Config;
  meta: {
    message: string;
  };
}

export enum ThemeMode {
  LIGHT = 'light',
  DARK = 'dark'
}

export enum MenuOrientation {
  VERTICAL = 'vertical',
  HORIZONTAL = 'horizontal'
}

export enum ThemeDirection {
  LTR = 'ltr',
  RTL = 'rtl'
}

export type DefaultConfigProps = {
  fontFamily: FontFamily;
  locale: I18n;
  menuOrientation: MenuOrientation;
  miniDrawer: boolean;
  container: boolean;
  mode: ThemeMode;
  presetColor: PresetColor;
  themeDirection: ThemeDirection;
};

export enum NavActionType {
  FUNCTION = 'function',
  LINK = 'link'
}
