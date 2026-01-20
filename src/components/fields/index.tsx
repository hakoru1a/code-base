import AutocompleteField from './autocomplete-field.tsx';
import DatePickerField from './date-picker-field.tsx';
import SelectField from './select-field.tsx';
import InputField from './input-field.tsx';
import RichField from './rich-field.tsx';
import NumberField from './number-field.tsx';

export const Field = {
  Autocomplete: AutocompleteField,
  DatePicker: DatePickerField,
  Select: SelectField,
  Text: InputField,
  Rich: RichField,
  Number: NumberField
};

export * from './types';
