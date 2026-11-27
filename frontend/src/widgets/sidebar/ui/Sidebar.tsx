import { useEffect, useRef, useState } from 'react';
import { FiltersPanel } from '@/features/catalog-filters';
import { Button } from '@/shared/ui';
import { useMediaQuery } from '@/shared/lib';
import styles from './Sidebar.module.css';

export function Sidebar() {
  const isMobile = useMediaQuery('(max-width: 768px)');
  const [open, setOpen] = useState(false);
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open, isMobile]);

  if (!isMobile) {
    return (
      <aside className={styles.sidebar} aria-label="Filters">
        <FiltersPanel />
      </aside>
    );
  }

  return (
    <>
      <Button variant="primary" onClick={() => setOpen(true)}>
        Filters
      </Button>
      <dialog
        ref={dialogRef}
        className={styles.drawer}
        aria-label="Filters"
        onClose={() => setOpen(false)}
      >
        <Button variant="ghost" onClick={() => setOpen(false)}>
          Close
        </Button>
        <FiltersPanel />
      </dialog>
    </>
  );
}
