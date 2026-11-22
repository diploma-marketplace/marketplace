import { Navigate } from 'react-router-dom';
import { Skeleton } from '@/shared/ui';
import { useAuth } from '@/features/auth';
import {
  ProfileInfoSection,
  DetailsSection,
  SecuritySection,
  PreferencesSection,
  OrdersSection,
  SellerSection,
  DangerZoneSection,
} from './sections';
import styles from './ProfilePage.module.css';

export function ProfilePage() {
  const { status, session } = useAuth();

  if (status === 'loading') {
    return (
      <div className={styles.page}>
        <Skeleton height={32} width={200} />
        <Skeleton height={140} />
        <Skeleton height={140} />
      </div>
    );
  }

  if (!session) return <Navigate to="/" replace />;

  // TODO: adjust role rules to the technical task.
  const isBuyer = session.role === 'User';
  const canSell = session.role === 'User' || session.role === 'Company';

  return (
    <div className={styles.page}>
      <h1 className={styles.title}>My profile</h1>

      <ProfileInfoSection session={session} />

      <div className={styles.grid}>
        <DetailsSection />
        <div className={styles.column}>
          <SecuritySection />
          <PreferencesSection />
        </div>
      </div>

      {isBuyer && <OrdersSection />}
      {canSell && <SellerSection />}

      <DangerZoneSection />
    </div>
  );
}
