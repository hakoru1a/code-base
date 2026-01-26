import { DialogContext, type DialogContextProps } from '@contexts/dialog';
import { useContext } from 'react';

const useDialog = () => {
  const context = useContext(DialogContext);

  if (!context) {
    // Fallback for pages rendered outside DialogProvider (e.g. during isolated rendering / router error boundaries).
    const fallback: DialogContextProps = {
      show: async () => ({ success: false }),
      confirm: async (props) => {
        const ok = globalThis?.confirm
          ? globalThis.confirm(props.description ? `${props.title}\n\n${props.description}` : props.title)
          : false;
        return { success: ok };
      },
      notify: async (props) => {
        globalThis?.alert?.(props.description ? `${props.title}\n\n${props.description}` : props.title);
        return { success: true };
      },
      success: async (props) => {
        globalThis?.alert?.(props.description ? `${props.title}\n\n${props.description}` : props.title);
        return { success: true };
      },
      error: async (props) => {
        globalThis?.alert?.(props.description ? `${props.title}\n\n${props.description}` : props.title);
        return { success: true };
      }
    };
    return fallback;
  }

  return context;
};

export default useDialog;
