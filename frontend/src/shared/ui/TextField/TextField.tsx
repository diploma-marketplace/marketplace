import { useId, type ComponentPropsWithoutRef } from 'react';
import styles from './TextField.module.css';

interface TextFieldProps extends ComponentPropsWithoutRef<'input'> {
  label: string;
  error?: string;
  visibleLabel?: boolean;
}

export function TextField({
  label,
  error,
  visibleLabel = false,
  id,
  className,
  ...rest
}: TextFieldProps) {
  const autoId = useId();
  const inputId = id ?? autoId;
  const errorId = `${inputId}-error`;

  return (
    <div className={[styles.field, visibleLabel ? styles.labeled : ''].filter(Boolean).join(' ')}>
      <label htmlFor={inputId} className={visibleLabel ? styles.label : 'visually-hidden'}>
        {label}
      </label>
      <input
        id={inputId}
        className={[styles.input, error ? styles.invalid : '', className].filter(Boolean).join(' ')}
        placeholder={visibleLabel ? undefined : label}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? errorId : undefined}
        {...rest}
      />
      {error && (
        <p id={errorId} className={styles.error}>
          {error}
        </p>
      )}
    </div>
  );
}
