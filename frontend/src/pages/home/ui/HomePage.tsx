import styles from './HomePage.module.css'
import {BannerSection, CategorySection, RecommendedSection} from './sections';

export function HomePage() {
  return (
    <div className={styles.page}>
      <h1 className="visually-hidden">FutureMarket</h1>
      <BannerSection />
      <CategorySection />
      <RecommendedSection />
    </div>
  );
}
