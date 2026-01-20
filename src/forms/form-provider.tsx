import type { UseFormReturn } from 'react-hook-form';

import React from 'react';
import { FormProvider as RHFForm } from 'react-hook-form';

export type FormProps = {
  onSubmit: (payload: Dynamic, event: Dynamic) => Promise<void>;
  children: React.ReactNode;
  methods: UseFormReturn<Dynamic>;
  disabledAutoSubmit?: boolean;
  slotProps?: {
    form?: React.FormHTMLAttributes<HTMLFormElement>;
  };
};

export function Form({ children, onSubmit, methods, disabledAutoSubmit = true, slotProps = {} }: FormProps) {
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && disabledAutoSubmit) {
      e.preventDefault();
    }
  };

  return (
    <RHFForm {...methods}>
      <form noValidate autoComplete="off" {...slotProps?.form} onSubmit={methods.handleSubmit(onSubmit)} onKeyDown={handleKeyDown}>
        {children}
      </form>
    </RHFForm>
  );
}
