import type { Dayjs, OpUnitType } from 'dayjs';

import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import duration from 'dayjs/plugin/duration';
import timezone from 'dayjs/plugin/timezone';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(utc);
dayjs.extend(duration);
dayjs.extend(relativeTime);
dayjs.extend(timezone);

export const durationTz = dayjs().utcOffset() / 60;

export type DatePickerFormat = Dayjs | Date | string | number | null | undefined;

export type DatePickerControl = Dayjs | null;

export const formatPatterns = {
  full: 'DD/MM/YYYY HH:mm:ss',
  dateTime: 'DD/MM/YYYY HH:mm',
  date: 'DD/MM/YYYY',
  time: 'HH:mm:ss',
  split: {
    dateTime: 'DD/MM/YYYY HH:mm:ss',
    date: 'DD/MM/YYYY'
  },
  paramCase: {
    dateTime: 'DD-MM-YYYY HH:mm:ss',
    date: 'DD-MM-YYYY'
  },
  iso: {
    dateTime: 'YYYY-MM-DDTHH:mm:ssZ',
    date: 'YYYY-MM-DD'
  }
};

export const MIN_DATE = '2000-01-01';
export const MAX_DATE = '2099-12-31';

const isValidDate = (date: DatePickerFormat) => date !== null && date !== undefined && dayjs(date).isValid();

/**
 * @output 2024-05-28T05:55:31+00:00
 */
export type DurationProps = {
  years?: number;
  months?: number;
  days?: number;
  hours?: number;
  minutes?: number;
  seconds?: number;
  milliseconds?: number;
};

export const dateHelper = {
  isValidDate,

  normalizeDateValue(value: DatePickerFormat): Dayjs | null {
    if (dayjs.isDayjs(value)) return value;

    const parsed = value ? dayjs(value) : null;
    return parsed?.isValid() ? parsed : null;
  },

  from(date: DatePickerFormat) {
    return dayjs(date);
  },

  now() {
    return dayjs(new Date());
  },

  today(template?: string): string {
    return dayjs(new Date())
      .startOf('day')
      .format(template ?? formatPatterns.full);
  },
  tomorrow() {
    return dayjs(new Date()).add(1, 'day');
  },
  /**
   * @output 17 Apr 2022 12:00 am
   */
  formatDateTimeFull(date: DatePickerFormat, pattern?: string): string {
    if (!isValidDate(date)) {
      return 'Invalid date';
    }

    return dayjs(date).format(pattern ?? formatPatterns.full);
  },

  /**
   * @output 17 Apr 2022 12:00 am
   */
  formatDateTime(date: DatePickerFormat, pattern?: string): string {
    if (!isValidDate(date)) {
      return 'Invalid date';
    }

    return dayjs(date).format(pattern ?? formatPatterns.dateTime);
  },

  /**
   * @output 17 Apr 2022
   */
  formatDate(date: DatePickerFormat, pattern?: string): string {
    if (!isValidDate(date)) {
      return 'Invalid date';
    }

    return dayjs(date).format(pattern ?? formatPatterns.date);
  },

  /**
   * @output 12:00 am
   */
  formatTime(date: DatePickerFormat, pattern?: string): string {
    if (!isValidDate(date)) {
      return 'Invalid date';
    }

    return dayjs(date).format(pattern ?? formatPatterns.time);
  },

  /**
   * @output 2025-02-13T14:12:09+07:00 -> 2025-02-13T07:12:09
   */
  toUtcTime(date: DatePickerFormat, pattern?: string): string {
    return dayjs.utc(date).format(pattern ?? formatPatterns.full);
  },

  /**
   * @output 2025-02-13T14:12:09+07:00 -> 2025-02-13T14:12:09
   */
  toLocalTime(date: DatePickerFormat, pattern?: string): string {
    return dayjs
      .utc(date)
      .tz(dayjs.tz.guess())
      .format(pattern ?? formatPatterns.full);
  },

  toTimeZone(date: DatePickerFormat, pattern?: string): string {
    return dayjs(date)
      .tz(dayjs.tz.guess())
      .format(pattern ?? formatPatterns.iso.dateTime);
  },

  /**
   * @output 1713250100
   */
  formatTimestamp(date: DatePickerFormat): number | 'Invalid date' {
    if (!isValidDate(date)) {
      return 'Invalid date';
    }

    return dayjs(date).valueOf();
  },

  /**
   * @output a few seconds, 2 years
   */
  formatToNow(date: DatePickerFormat): string {
    if (!isValidDate(date)) {
      return 'Invalid date';
    }

    return dayjs(date).toNow(true);
  },

  /**
   * @output boolean
   */
  formatIsBetween(inputDate: DatePickerFormat, startDate: DatePickerFormat, endDate: DatePickerFormat): boolean {
    if (!isValidDate(inputDate) || !isValidDate(startDate) || !isValidDate(endDate)) {
      return false;
    }

    const formattedInputDate = this.formatTimestamp(inputDate);
    const formattedStartDate = this.formatTimestamp(startDate);
    const formattedEndDate = this.formatTimestamp(endDate);

    if (formattedInputDate === 'Invalid date' || formattedStartDate === 'Invalid date' || formattedEndDate === 'Invalid date') {
      return false;
    }

    return formattedInputDate >= formattedStartDate && formattedInputDate <= formattedEndDate;
  },

  formatIsBefore(startDate: DatePickerFormat, endDate: DatePickerFormat): boolean {
    if (!isValidDate(startDate) || !isValidDate(endDate)) {
      return false;
    }

    return dayjs(endDate).isBefore(startDate);
  },

  /**
   * @output boolean
   */
  formatIsAfter(startDate: DatePickerFormat, endDate: DatePickerFormat): boolean {
    if (!isValidDate(startDate) || !isValidDate(endDate)) {
      return false;
    }

    return dayjs(startDate).isAfter(endDate);
  },
  /**
   * @output boolean
   */
  formatIsSame(startDate: DatePickerFormat, endDate: DatePickerFormat, unitToCompare?: OpUnitType): boolean {
    if (!isValidDate(startDate) || !isValidDate(endDate)) {
      return false;
    }

    return dayjs(startDate).isSame(endDate, unitToCompare ?? 'year');
  },

  /**
   * Check if date is within the specified range (inclusive)
   * @param date - Date to check
   * @param minDate - Minimum date (inclusive)
   * @param maxDate - Maximum date (inclusive)
   * @returns boolean
   */
  isInDateRange(date: DatePickerFormat, minDate: DatePickerFormat, maxDate: DatePickerFormat): boolean {
    if (!isValidDate(date) || !isValidDate(minDate) || !isValidDate(maxDate)) {
      return false;
    }

    const dateValue = dayjs(date);
    const minValue = dayjs(minDate);
    const maxValue = dayjs(maxDate);

    return (
      (dateValue.isAfter(minValue) || dateValue.isSame(minValue, 'day')) &&
      (dateValue.isBefore(maxValue) || dateValue.isSame(maxValue, 'day'))
    );
  },

  /**
   * @output
   * Same day: 26 Apr 2024
   * Same month: 25 - 26 Apr 2024
   * Same month: 25 - 26 Apr 2024
   * Same year: 25 Apr - 26 May 2024
   */
  formatDateRangeShortLabel(startDate: DatePickerFormat, endDate: DatePickerFormat, initial?: boolean): string {
    if (!isValidDate(startDate) || !isValidDate(endDate) || this.formatIsAfter(startDate, endDate)) {
      return 'Invalid date';
    }

    let label = `${this.formatDate(startDate)} - ${this.formatDate(endDate)}`;

    if (initial) {
      return label;
    }

    const isSameYear = this.formatIsSame(startDate, endDate, 'year');
    const isSameMonth = this.formatIsSame(startDate, endDate, 'month');
    const isSameDay = this.formatIsSame(startDate, endDate, 'day');

    if (isSameYear && !isSameMonth) {
      label = `${this.formatDate(startDate, 'DD MMM')} - ${this.formatDate(endDate)}`;
    } else if (isSameYear && isSameMonth && !isSameDay) {
      label = `${this.formatDate(startDate, 'DD')} - ${this.formatDate(endDate)}`;
    } else if (isSameYear && isSameMonth && isSameDay) {
      label = `${this.formatDate(endDate)}`;
    }

    return label;
  },

  addDay(date: DatePickerFormat, days: number): Dayjs | null {
    if (!isValidDate(date)) {
      return null;
    }

    return dayjs(date).add(days, 'day');
  },

  formatAdd({ years = 0, months = 0, days = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0 }: DurationProps) {
    return dayjs()
      .add(
        dayjs.duration({
          years,
          months,
          days,
          hours,
          minutes,
          seconds,
          milliseconds
        })
      )
      .format();
  },

  /**
   * @output 2024-05-28T05:55:31+00:00
   */
  formatSub({ years = 0, months = 0, days = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0 }: DurationProps) {
    return dayjs()
      .subtract(
        dayjs.duration({
          years,
          months,
          days,
          hours,
          minutes,
          seconds,
          milliseconds
        })
      )
      .format();
  },

  /**
   * Convert Date object to YYYY-MM-DD format without timezone issues
   * @param date - Date object or dayjs object
   * @returns string in YYYY-MM-DD format
   */
  toDateString(date: Date | Dayjs): string {
    const d = dayjs(date);
    const year = d.year();
    const month = String(d.month() + 1).padStart(2, '0');
    const day = String(d.date()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  },

  /**
   * Convert Date object to local date string without timezone conversion
   * @param date - Date object
   * @returns string in YYYY-MM-DD format
   */
  toLocalDateString(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
};
