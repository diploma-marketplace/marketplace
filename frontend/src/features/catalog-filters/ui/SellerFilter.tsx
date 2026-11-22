import type { SellerType } from '@/entities/product';
import styles from './Filters.module.css';

const OPTIONS: { value: SellerType; label: string }[] = [
  { value: 'marketplace', label: 'From marketplace' },
  { value: 'company', label: 'From companies' },
  { value: 'fop', label: 'From private sellers' },
];

interface Props {
  value: SellerType[];
  onChange: (next: SellerType[]) => void;
}

export function SellerFilter({ value, onChange }: Props) {
  const toggle = (s: SellerType) =>
    onChange(value.includes(s) ? value.filter((v) => v !== s) : [...value, s]);

  return (
    <fieldset className={`${styles.group} ${styles.sellers}`}>
      <legend className={styles.legend}>Seller</legend>
      {OPTIONS.map((o) => (
        <label
          key={o.value}
          className={o.value === 'marketplace' ? `${styles.check} ${styles.primaryCheck}` : styles.check}
        >
          <input type="checkbox" checked={value.includes(o.value)} onChange={() => toggle(o.value)} />
          {o.label}
        </label>
      ))}
    </fieldset>
  );
}