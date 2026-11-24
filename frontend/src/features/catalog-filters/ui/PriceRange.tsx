import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import styles from './Filters.module.css';

interface Props {
  bounds: { min: number; max: number };
  min?: number;
  max?: number;
  onCommit: (min?: number, max?: number) => void;
}

// Ремонтуйте з key={`${bounds.min}-${bounds.max}-${min}-${max}`}, щоб локальний стан синхронізувався з URL.
export function PriceRange({ bounds, min, max, onCommit }: Props) {
  const { t } = useTranslation();
  const [lo, setLo] = useState(min ?? bounds.min);
  const [hi, setHi] = useState(max ?? bounds.max);

  const commit = () =>
    onCommit(lo <= bounds.min ? undefined : lo, hi >= bounds.max ? undefined : hi);

  const shared = {
    type: 'range' as const,
    min: bounds.min,
    max: bounds.max,
    step: 50,
    onPointerUp: commit,
    onKeyUp: commit,
  };

  return (
    <fieldset className={styles.group}>
      <legend className={styles.legend}>{t('catalog.price')}</legend>
      <label className={styles.range}>
        <span>{t('catalog.from')} {lo}</span>
        <input
          {...shared}
          value={lo}
          onChange={(e) => setLo(Math.min(Number(e.target.value), hi))}
          aria-label={t('catalog.from')}
        />
      </label>
      <label className={styles.range}>
        <span>{t('catalog.to')} {hi}</span>
        <input
          {...shared}
          value={hi}
          onChange={(e) => setHi(Math.max(Number(e.target.value), lo))}
          aria-label={t('catalog.to')}
        />
      </label>
    </fieldset>
  );
}