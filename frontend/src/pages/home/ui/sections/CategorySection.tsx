import { Container } from '@/shared/ui';
// import {CategoryCard} from '@/entities/category';
import styles from './sections.module.css';

export function CategorySection() {
  return (
    <section className={styles.section}>
      <Container>
        <h2 className={styles.title}>Categories</h2>
        <div className={styles.gridSmall}>{/*TBA: mock categories*/}</div>
      </Container>
    </section>
  );
}
