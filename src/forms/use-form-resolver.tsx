import type zod from 'zod';
import type { Resolver, FieldValues, UseFormProps } from 'react-hook-form';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

export function useFormResolver<TFieldValues extends FieldValues = FieldValues, TContext = Dynamic>(
  schema: zod.ZodSchema<TFieldValues, FieldValues>,
  options?: Omit<UseFormProps<TFieldValues, TContext>, 'resolver'>
) {
  return useForm<TFieldValues, TContext>({
    mode: 'all',
    reValidateMode: 'onChange',
    ...options,
    resolver: zodResolver(schema) as unknown as Resolver<TFieldValues, TContext>
  });
}
