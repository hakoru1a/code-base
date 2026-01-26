import React, { useCallback, useState } from 'react';

type ReturnType = {
  value: boolean;
  onTrue: () => void;
  onFalse: () => void;
  onToggle: () => void;
  setValue: React.Dispatch<React.SetStateAction<boolean>>;
};

/**
 * Simple boolean toggle hook.
 * Kept for convention usage (accordion toggles, etc).
 */
const useToggle = (defaultValue?: boolean): ReturnType => {
  const [value, setValue] = useState(!!defaultValue);

  const onTrue = useCallback(() => setValue(true), []);
  const onFalse = useCallback(() => setValue(false), []);
  const onToggle = useCallback(() => setValue((prev) => !prev), []);

  return { value, onTrue, onFalse, onToggle, setValue };
};

export default useToggle;
