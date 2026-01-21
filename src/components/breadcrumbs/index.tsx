import { Link } from 'react-router-dom';

import { useTheme } from '@mui/material/styles';
import Divider from '@mui/material/Divider';
import Grid from '@mui/material/Grid';
import MuiBreadcrumbs, { BreadcrumbsProps } from '@mui/material/Breadcrumbs';

import ApartmentOutlined from '@ant-design/icons/ApartmentOutlined';
import { OverrideIcon } from '@themes';
import { ThemeDirection } from '@contexts/config';
import { MainCard } from '../cards';
import { useTranslate } from '@locales';
import Text from '../text';

interface BreadcrumbLinkProps {
  title: string;
  to?: string;
  icon?: string | OverrideIcon;
}

type Props = {
  heading: string;
  card?: boolean;
  divider?: boolean;
  links?: BreadcrumbLinkProps[];
  maxItems?: number;
  rightAlign?: boolean;
  separator?: OverrideIcon;
  title?: boolean;
  titleBottom?: boolean;
  sx?: BreadcrumbsProps['sx'];
};

export default function Breadcrumbs({
  heading,
  card = false,
  divider = false,
  links,
  maxItems,
  rightAlign,
  separator,
  title = true,
  titleBottom = true,
  sx,
  ...others
}: Props) {
  const { t } = useTranslate();
  const theme = useTheme();

  const iconSX = {
    marginRight: theme.direction === ThemeDirection.RTL ? 0 : theme.spacing(0.75),
    marginLeft: theme.direction === ThemeDirection.RTL ? theme.spacing(0.75) : 0,
    width: '1rem',
    height: '1rem',
    color: theme.palette.secondary.main
  };

  // item separator
  const SeparatorIcon = separator!;
  const separatorIcon = separator ? <SeparatorIcon style={{ fontSize: '0.75rem', marginTop: 2 }} /> : '/';

  let CollapseIcon;

  return (
    <MainCard
      border={card}
      sx={card === false ? { mb: 3, bgcolor: 'inherit', backgroundImage: 'none', ...sx } : { mb: 3, ...sx }}
      {...others}
      content={card}
      shadow="none"
    >
      <Grid
        container
        direction={rightAlign ? 'row' : 'column'}
        justifyContent={rightAlign ? 'space-between' : 'flex-start'}
        alignItems={rightAlign ? 'center' : 'flex-start'}
        spacing={1}
      >
        {title && !titleBottom && (
          <Grid>
            <Text.Typography label={heading} variant="h2" />
          </Grid>
        )}
        <Grid>
          <MuiBreadcrumbs aria-label="breadcrumb" maxItems={maxItems || 8} separator={separatorIcon}>
            {links?.map((link: BreadcrumbLinkProps, index: number) => {
              CollapseIcon = link.icon ? link.icon : ApartmentOutlined;

              return (
                <Text.Typography
                  key={index}
                  {...(link.to && { component: Link, to: link.to })}
                  variant={!link.to ? 'subtitle1' : 'h6'}
                  sx={{ textDecoration: 'none' }}
                  color={!link.to ? 'text.primary' : 'text.secondary'}
                  label={
                    <>
                      {link.icon && <CollapseIcon style={iconSX} />}
                      {t(link.title as string)}
                    </>
                  }
                />
              );
            })}
          </MuiBreadcrumbs>
        </Grid>
        {title && titleBottom && (
          <Grid sx={{ mt: card === false ? 0.25 : 1 }}>
            <Text.Typography variant="h2" label={heading} />
          </Grid>
        )}
      </Grid>
      {card === false && divider !== false && <Divider sx={{ mt: 2 }} />}
    </MainCard>
  );
}
