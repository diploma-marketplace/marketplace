import type { AuthSession } from '@/features/auth';
import {
  mapProfileError,
  toUpdatePayload,
  useMyProfile,
  useUpdateProfile,
} from '@/features/profile';
import { ProfileSection } from './ProfileSection';
import { AvatarUpload } from './AvatarUpload';
import styles from './ProfileInfoSection.module.css';

export function ProfileInfoSection({ session }: { session: AuthSession }) {
  const { data: profile } = useMyProfile();
  const update = useUpdateProfile();

  const removeError = update.isError ? mapProfileError(update.error).formError : null;

  return (
    <ProfileSection title="Account">
      <div className={styles.row}>
        <AvatarUpload
          avatarUrl={profile?.avatarUrl ?? session.avatarUrl}
          fullName={profile?.fullName ?? session.fullName}
          removing={update.isPending}
          onRemove={
            profile ? () => update.mutate(toUpdatePayload(profile, { avatarUrl: null })) : undefined
          }
        />

        <div className={styles.info}>
          <p className={styles.name}>{profile?.fullName ?? session.fullName}</p>
          <p className={styles.email}>{session.email}</p>
          <p className={styles.meta}>
            Role: {session.role} · Verification: {session.verificationStatus}
          </p>
          {profile && (
            <p className={styles.meta}>
              Rating {profile.rating.toFixed(1)} ({profile.reviewsCount} reviews) · Member since{' '}
              {new Date(profile.createdAt).toLocaleDateString()}
            </p>
          )}
        </div>
      </div>

      {removeError && (
        <p className={styles.error} role="alert">
          {removeError}
        </p>
      )}
    </ProfileSection>
  );
}
