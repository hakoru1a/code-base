import { useRef, useState } from 'react';

// material-ui
import useMediaQuery from '@mui/material/useMediaQuery';
import ClickAwayListener from '@mui/material/ClickAwayListener';
import Grid from '@mui/material/Grid';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import Paper from '@mui/material/Paper';
import Popper from '@mui/material/Popper';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';

import TranslationOutlined from '@ant-design/icons/TranslationOutlined';
import { I18n, useTranslate } from '@locales';
import { Button, Transitions } from '@components';
import { useConfig } from '@hooks';
import { LANGUAGES } from '@utils/constants';

export default function Localization() {
  const { i18n } = useTranslate();
  const downMD = useMediaQuery((theme) => theme.breakpoints.down('md'));

  const { locale, onChangeLocalization } = useConfig();

  const anchorRef = useRef<Dynamic>(null);
  const [open, setOpen] = useState(false);
  const handleToggle = () => {
    setOpen((prevOpen) => !prevOpen);
  };

  const handleClose = (event: MouseEvent | TouchEvent) => {
    if (anchorRef.current && anchorRef.current.contains(event.target)) {
      return;
    }
    setOpen(false);
  };

  const handleListItemClick = async (lang: I18n) => {
    onChangeLocalization(lang);
    setOpen(false);
    await i18n.changeLanguage(lang);
  };

  return (
    <Box sx={{ flexShrink: 0, ml: 0.75 }}>
      <Button.Icon
        color="secondary"
        variant="light"
        sx={(theme) => ({
          color: 'text.primary',
          bgcolor: open ? 'grey.100' : 'transparent',
          ...theme.applyStyles('dark', { bgcolor: open ? 'background.default' : 'transparent' })
        })}
        aria-label="open localization"
        ref={anchorRef}
        aria-controls={open ? 'localization-grow' : undefined}
        aria-haspopup="true"
        onClick={handleToggle}
      >
        <TranslationOutlined />
      </Button.Icon>
      <Popper
        placement={downMD ? 'bottom-start' : 'bottom'}
        open={open}
        anchorEl={anchorRef.current}
        role={undefined}
        transition
        disablePortal
        popperOptions={{ modifiers: [{ name: 'offset', options: { offset: [downMD ? 0 : 0, 9] } }] }}
      >
        {({ TransitionProps }) => (
          <Transitions type="grow" position={downMD ? 'top-right' : 'top'} in={open} {...TransitionProps}>
            <Paper sx={(theme) => ({ boxShadow: theme.customShadows.z1 })}>
              <ClickAwayListener onClickAway={handleClose}>
                <List
                  component="nav"
                  sx={{
                    p: 0,
                    width: '100%',
                    minWidth: 200,
                    maxWidth: { xs: 250, md: 290 },
                    bgcolor: 'background.paper',
                    borderRadius: 0.5
                  }}
                >
                  {LANGUAGES.map((language) => (
                    <ListItemButton
                      key={language.code}
                      selected={locale === language.code}
                      onClick={() => handleListItemClick(language.code)}
                    >
                      <ListItemText
                        primary={
                          <Grid container>
                            <Typography color="text.primary">{language.name}</Typography>
                            <Typography variant="caption" color="text.secondary" sx={{ ml: '8px' }}>
                              ({language.code.toUpperCase()})
                            </Typography>
                          </Grid>
                        }
                      />
                    </ListItemButton>
                  ))}
                </List>
              </ClickAwayListener>
            </Paper>
          </Transitions>
        )}
      </Popper>
    </Box>
  );
}
