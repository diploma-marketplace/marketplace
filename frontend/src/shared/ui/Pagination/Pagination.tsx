import { Button } from '../Button';
import styles from './Pagination.module.css';
import { useTranslation } from 'react-i18next';

interface Props {
  page: number;
  pageCount: number;
  onChange: (page: number) => void;
}

function pageItems(page: number, count: number): (number | 'gap')[] {
  const pages = [...new Set([1, page - 1, page, page + 1, count])]
    .filter((p) => p >= 1 && p <= count)
    .sort((a, b) => a - b);
  const out: (number | 'gap')[] = [];
  pages.forEach((p, i) => {
    if (i > 0 && p - pages[i - 1] > 1) out.push('gap');
    out.push(p);
  });
  return out;
}

export function Pagination({ page, pageCount, onChange }: Props) {
  const { t } = useTranslation();
  if (pageCount <= 1) return null;
  return (
    <nav className={styles.nav} aria-label="Pagination">
      <Button variant="ghost" disabled={page === 1} onClick={() => onChange(page - 1)}>
        {t('catalog.prev')}
      </Button>
      {pageItems(page, pageCount).map((item, i) =>
        item === 'gap' ? (
          <span key={`gap-${i}`} aria-hidden="true">
            …
          </span>
        ) : (
          <Button
            key={item}
            variant={item === page ? 'primary' : 'ghost'}
            aria-current={item === page ? 'page' : undefined}
            aria-label={`Page ${item}`}
            onClick={() => onChange(item)}
          >
            {item}
          </Button>
        ),
      )}
      <Button variant="ghost" disabled={page === pageCount} onClick={() => onChange(page + 1)}>
        {t('catalog.next')}
      </Button>
    </nav>
  );
}