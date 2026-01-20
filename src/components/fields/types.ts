import type { TextFieldProps } from '@mui/material/TextField';
import type { AutocompleteProps, StackProps, TypographyProps, MenuItemProps, FormControlLabelProps, CheckboxProps } from '@mui/material';
import type { DatePickerProps } from '@mui/lab';
import { Dayjs } from 'dayjs';
import React, { type ReactNode } from 'react';
import type { NumericFormatProps as BaseNumericFormatProps } from 'react-number-format';

export type MaxLengthInputProps = {
  maxLength?: number;
  showLength?: boolean;
};

export type BaseInputProps = MaxLengthInputProps & TextFieldProps;

export type InputLabelProps = {
  label?: string;
  required?: boolean;
};

export type InputLabelSlotProps = Omit<TypographyProps, 'label'>;

export type FormInputLabelSlotProps = {
  container?: StackProps;
  label?: InputLabelSlotProps;
};

export type InputFieldProps = Omit<BaseInputProps, 'onChange' | 'type'> &
  InputLabelProps & {
    onChange?: (value: string | number) => void;
    required?: boolean;
    readOnly?: boolean;
    type?: string;
    slotProps?: Pick<BaseInputProps, 'slotProps'> & FormInputLabelSlotProps;
  };

export type RichFieldProps = MaxLengthInputProps & Omit<TextFieldProps, 'multiline'>;

export type NumericFormatProps = Pick<
  BaseNumericFormatProps,
  | 'thousandSeparator'
  | 'decimalScale'
  | 'decimalSeparator'
  | 'allowLeadingZeros'
  | 'allowNegative'
  | 'allowedDecimalSeparators'
  | 'max'
  | 'min'
  | 'isAllowed'
  | 'onBlur'
>;

export type NumberFieldProps = Omit<InputFieldProps, 'onChange'> & {
  onChange?: (value: string) => void;
  slotProps?: Pick<InputFieldProps, 'slotProps'> & {
    number?: NumericFormatProps;
  };
};

export type NumberFormatProps = NumericFormatProps & {
  onChange: (event: { target: { value: string } }) => void;
};

export type AutocompleteFieldProps<
  T,
  Multiple extends boolean | undefined,
  DisableClearable extends boolean | undefined,
  FreeSolo extends boolean | undefined
> = Omit<AutocompleteProps<T, Multiple, DisableClearable, FreeSolo>, 'renderInput' | 'onChange'> &
  InputLabelProps & {
    placeholder?: string;
    error?: boolean;
    helperText?: string;
    fullWidth?: boolean;
    onChange?: (value: T) => void;
    onSearch?: (value: string) => Promise<T[]>;
    slotProps?: Pick<AutocompleteProps<Dynamic, undefined, undefined, undefined>, 'slotProps'> &
      FormInputLabelSlotProps & {
        textfield?: TextFieldProps;
      };
  };

export type SelectFieldProps<TData> = Omit<InputFieldProps, 'onChange'> & {
  options: TData[];
  defaultOptionLabel?: string;
  labelOption?: string;
  valueOption?: string;
  defaultOption?: boolean;
  onChange?: (data: string | number) => void;
  renderLabel?: (data: TData) => React.ReactNode;
  slotProps?: Pick<InputFieldProps, 'slotProps'> & {
    item?: MenuItemProps;
  };
};

export type DatePickerDayjs = DatePickerProps<Dayjs>;

export type DatePickerFieldProps = DatePickerDayjs &
  InputLabelProps & {
    placeholder?: string;
    error?: boolean;
    helperText?: ReactNode;
    fullWidth?: boolean;
    customProps?: FormInputLabelSlotProps & {
      textField?: TextFieldProps;
    };
  };

export type CheckboxFieldProps = Omit<CheckboxProps, 'onChange' | 'checked'> & {
  label: React.ReactNode;
  checked?: boolean | null;
  onChange?: (value: boolean | null) => void;
  acceptNull?: boolean;
  controlProps?: Omit<FormControlLabelProps, 'control' | 'label'>;
};
