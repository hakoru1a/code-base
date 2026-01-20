import { Box, TextField, Typography } from '@mui/material';
import React, { useMemo } from 'react';
import type { RichFieldProps } from './types';

const RichField = React.forwardRef(
  ({ value, maxLength, inputProps, showLength = true, fullWidth = true, rows = 4, ...restProps }: RichFieldProps, ref) => {
    const hasShowLengthText = useMemo(() => !!maxLength && showLength, [maxLength, showLength]);

    return (
      <Box
        sx={{
          position: 'relative',
          ...(fullWidth && {
            width: '100%'
          })
        }}
      >
        <TextField
          fullWidth={fullWidth}
          size="small"
          variant="outlined"
          inputRef={ref}
          inputProps={{ maxLength: maxLength, ...inputProps }}
          value={value}
          {...restProps}
          rows={rows}
          multiline
          InputProps={{
            sx: { paddingBottom: hasShowLengthText ? '30px' : '8.5px' },
            ...restProps.InputProps
          }}
        />
        {hasShowLengthText && (
          <Typography
            variant="caption"
            sx={{
              position: 'absolute',
              bottom: 5,
              right: 14,
              color: 'text.secondary'
            }}
          >
            `${`${value}`.length} / ${maxLength}`
          </Typography>
        )}
      </Box>
    );
  }
);

export default RichField;
