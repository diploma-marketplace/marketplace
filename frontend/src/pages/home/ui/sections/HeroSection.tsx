import { Container, Button } from '@/shared/ui';
import styles from './sections.module.css';

export function HeroSection() {
  return (
    <section className={styles.hero}>
      <Container>
        <h1 className={styles.heroTitle}>A marketplace without unnecessary noise</h1>
        <p className={styles.heroText}>
          Fair prices, verified reviews, comfortable product comparison.
        </p>
        <Button variant="primary">Go to catalogue</Button>
      </Container>
    </section>
  );
}
