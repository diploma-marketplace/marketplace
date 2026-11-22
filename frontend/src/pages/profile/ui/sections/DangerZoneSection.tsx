import { ProfileSection } from './ProfileSection';

export function DangerZoneSection() {
  return (
    <ProfileSection title="Danger zone">
      {/* TODO: deactivate account (DELETE /api/users/me) with a confirmation step, then log the user out.*/}
      <p>Deactivate your account.</p>
    </ProfileSection>
  );
}
