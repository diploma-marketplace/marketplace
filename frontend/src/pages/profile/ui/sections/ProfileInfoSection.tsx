import type { AuthSession } from '@/features/auth';
import { ProfileSection } from './ProfileSection';
import styles from './ProfileInfoSection.module.css';

export function ProfileInfoSection({ session }: { session: AuthSession }) {
  return (
    <ProfileSection title="Account">
      <div className={styles.row}>
        <div className={styles.avatar} aria-hidden="true">
          {session.avatarUrl ? (
            <img src={session.avatarUrl} alt="Avatar" className={styles.avatarImage} />
          ) : (
            session.fullName.charAt(0).toUpperCase()
          )}
        </div>

        <div className={styles.info}>
          <p className={styles.name}>{session.fullName}</p>
          <p className={styles.email}>{session.email}</p>
          <p className={styles.meta}>
            Role: {session.role} · Verification: {session.verificationStatus}
          </p>
        </div>
      </div>
    </ProfileSection>
  );
}
