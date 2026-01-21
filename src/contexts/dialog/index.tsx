import merge from 'lodash-es/merge';
import React, { ComponentType, Fragment } from 'react';

import { createContext, useState } from 'react';
import { stringHelper } from '@utils/helpers';
import { ConfirmDialog, ConfirmDialogProps, NotifyDialog, NotifyDialogProps } from '@components';

export type DialogRequest = {
  visible: boolean;
  onClose: (payload?: Partial<DialogResult<Dynamic>> | null) => void;
};

export type DialogResult<TResult> = {
  success: boolean;
  payload: TResult;
};

type ComponentProps = {
  key: string;
  component: React.ReactNode;
  visible: boolean;
};

export type DialogContextProps = {
  show: <P extends DialogRequest>(
    Component: ComponentType<P>,
    props?: Omit<P, 'visible' | 'onClose'>
  ) => Promise<Partial<DialogResult<Dynamic>>>;
  confirm: (props: ConfirmDialogProps) => Promise<Partial<DialogResult<Dynamic>>>;
  notify: (props: NotifyDialogProps) => Promise<Partial<DialogResult<Dynamic>>>;
  error: (props: NotifyDialogProps) => Promise<Partial<DialogResult<Dynamic>>>;
  success: (props: NotifyDialogProps) => Promise<Partial<DialogResult<Dynamic>>>;
};

export const DialogContext = createContext<DialogContextProps | undefined>(undefined);

export const DialogProvider = ({ children }: { children: React.ReactNode }) => {
  const [components, setComponents] = useState<ComponentProps[]>([]);

  const show = async <P extends DialogRequest>(Component: Dynamic, props?: Omit<P, 'visible' | 'onClose'>) => {
    if (!Component) {
      throw new Error('Component is required');
    }

    return new Promise<Partial<DialogResult<Dynamic>>>((resolve) => {
      const dialogKey = stringHelper.newGuid();
      const onClose = (result?: Partial<DialogResult<Dynamic>> | null) => {
        setComponents((prev) => prev.map((c) => (c.key === dialogKey ? { ...c, visible: false } : c)));

        setTimeout(() => {
          setComponents((prev) => prev.filter((component) => component.key !== dialogKey));
        }, 300);

        resolve(result || { success: false });
      };

      const defaultProps: DialogRequest = {
        onClose,
        visible: true
      };

      const newProps = { ...props, ...defaultProps };
      const newComponent = <Component {...newProps} />;
      setComponents((prev) => [...prev, { key: dialogKey, component: newComponent, visible: true }]);
    });
  };

  const confirm = async (props: ConfirmDialogProps) => show(ConfirmDialog, props);

  const notify = async (props: NotifyDialogProps) => show(NotifyDialog, props);

  const success = async (props: NotifyDialogProps) =>
    show(
      NotifyDialog,
      merge(
        {
          slotProps: {
            accept: {
              show: true,
              color: 'success'
            },
            reject: {
              show: true
            }
          }
        },
        props
      )
    );

  const error = async (props: NotifyDialogProps) =>
    show(
      NotifyDialog,
      merge(
        {
          slotProps: {
            accept: {
              show: true,
              color: 'error'
            },
            reject: {
              show: false
            }
          }
        },
        props
      )
    );

  return (
    <DialogContext.Provider
      value={{
        show,
        confirm,
        notify,
        error,
        success
      }}
    >
      {children}
      {components.map(({ key, component: Component, visible }) => (
        <Fragment key={key}>{React.cloneElement(Component as React.ReactElement<Pick<DialogRequest, 'visible'>>, { visible })}</Fragment>
      ))}
    </DialogContext.Provider>
  );
};
