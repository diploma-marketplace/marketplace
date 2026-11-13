import { Container } from '@/shared/ui';
// import {ProductCard} from "@/entities/product";
import styles from './sections.module.css';

export function RecommendedSection() {
  //const isLoading = false; // TBA: API connection

  return (
    <section className={styles.section}>
      <Container>
        <h2 className={styles.title}>We recommend</h2>
        <div className={styles.grid}>
          {/*{isLoading
                        ? Array.from({length: 4}, (_, i) => <Skeleton key{i} height={260} />)
                        : mockProducts.map((p) => <ProductCard key={p.id} product={p} />)}*/}
        </div>
      </Container>
    </section>
  );
}
