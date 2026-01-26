import { Chip, Stack, Tab, Tabs } from '@mui/material';
import { alpha, useTheme } from '@mui/material/styles';
import type { ColorProps } from '@themes';

export type TabFilterOption<TValue extends string> = {
  label: string;
  value: TValue;
  count?: number;
  color?: ColorProps;
};

type Props<TValue extends string> = {
  value: TValue;
  onChange: (value: TValue) => void;
  options: TabFilterOption<TValue>[];
  size?: 'small' | 'medium';
};

const TabFilter = <TValue extends string>({ value, onChange, options, size = 'medium' }: Props<TValue>) => {
  const theme = useTheme();

  const getMainColor = (color?: ColorProps) => {
    if (!color || color === 'default' || color === 'inherit') return theme.palette.primary.main;
    const p = (theme.palette as Dynamic)[color];
    return p?.main || theme.palette.primary.main;
  };

  return (
    <Tabs
      value={value}
      onChange={(_, v) => onChange(v as TValue)}
      variant="scrollable"
      scrollButtons="auto"
      sx={{
        minHeight: 40,
        '& .MuiTabs-indicator': {
          height: 3,
          borderRadius: 2
        }
      }}
    >
      {options.map((opt) =>
        (() => {
          const selected = value === opt.value;
          const main = getMainColor(opt.color);
          const chipColor = (opt.color || 'default') as Dynamic;

          return (
            <Tab
              key={opt.value}
              value={opt.value}
              sx={{
                minHeight: 40,
                textTransform: 'none',
                borderRadius: 1,
                px: 1.5,
                mr: 1,
                ...(selected
                  ? {
                      backgroundColor: alpha(main, 0.12),
                      color: main,
                      fontWeight: 700
                    }
                  : {
                      color: theme.palette.text.secondary
                    })
              }}
              label={
                <Stack direction="row" spacing={1} alignItems="center">
                  <span>{opt.label}</span>
                  {typeof opt.count === 'number' && (
                    <Chip label={opt.count} size={size} color={chipColor} variant={selected ? 'filled' : 'outlined'} />
                  )}
                </Stack>
              }
            />
          );
        })()
      )}
    </Tabs>
  );
};

export default TabFilter;
