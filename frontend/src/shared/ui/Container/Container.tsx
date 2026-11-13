import type { HTMLAttributes } from 'react';
import styles from './Container.module.css';

export function Container({ className, ...rest }: HTMLAttributes<HTMLDivElement>) {
  return <div className={[styles.container, className].filter(Boolean).join(' ')} {...rest} />;
}
