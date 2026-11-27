import { ProfileSection } from './ProfileSection';
import { Skeleton, Button } from '@/shared/ui';
import { useMyProfile } from '@/features/profile';
import { ContactsForm } from '@/pages/profile/ui/sections/ContactsForm.tsx';

export function DetailsSection() {
  const { data: profile, isPending, isError, refetch } = useMyProfile();

  return (
    <ProfileSection title="Personal details">
      {isPending && (
        <>
          <Skeleton height={40} />
          <Skeleton height={40} />
          <Skeleton height={40} />
        </>
      )}

      {isError && (
        <>
          <p role="alert">Could not load details.</p>
          <Button variant="secondary" onClick={() => void refetch()}>
            Try again
          </Button>
        </>
      )}

      {profile && <ContactsForm profile={profile} />}
    </ProfileSection>
  );
}
