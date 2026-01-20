import Stack from '@mui/material/Stack';
import { CircularWithPath } from '../progress';

const CircularLoader = () => {
  return (
    <Stack sx={{ alignItems: 'center', justifyContent: 'center', height: 1 }}>
      <CircularWithPath />
    </Stack>
  );
};

export default CircularLoader;
