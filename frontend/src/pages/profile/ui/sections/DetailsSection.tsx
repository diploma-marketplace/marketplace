import { ProfileSection } from './ProfileSection';

export function DetailsSection() {
  return (
    <ProfileSection title="Personal details">
      {/*TODO: controlled form to be able to change data. Load: GET /api/users/me, save: PUT /api/users/me (updateProfileRequest*/}
      <p>Name, phone, address, social links</p>
    </ProfileSection>
  );
}
