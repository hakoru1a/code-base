import { RHFTextField } from './rhf-text-field';
import { RHFAutocomplete } from './rhf-autocomplete';
import RHFSelect from './rhf-select';
import { RHFDatePicker } from './rhf-date-picker';
import { RHFNumberField } from './rhf-number-field';

export const FormField = {
  Text: RHFTextField,
  Number: RHFNumberField,
  Select: RHFSelect,
  Autocomplete: RHFAutocomplete,
  DatePicker: RHFDatePicker
};
