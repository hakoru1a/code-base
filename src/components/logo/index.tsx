import { Link } from 'react-router-dom';
import { To } from 'history';

import { SxProps } from '@mui/material/styles';
import ButtonBase from '@mui/material/ButtonBase';

import Logo from './LogoMain';
import LogoIcon from './LogoIcon';
import { routes } from '@routes';

interface Props {
  reverse?: boolean;
  isIcon?: boolean;
  sx?: SxProps;
  to?: To;
}

export default function LogoSection({ reverse, isIcon, sx, to }: Props) {
  return (
    <ButtonBase disableRipple component={Link} to={to || routes.base} sx={sx}>
      {isIcon ? <LogoIcon /> : <Logo reverse={reverse} />}
    </ButtonBase>
  );
}
