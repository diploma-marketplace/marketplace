import { ProfileSection } from './ProfileSection';

export function SecuritySection() {
  return (
    <ProfileSection title="Security">
      {/*TODO: Add password changing: current password + new password + confirm new password, reusing validation from /auth/model/validation.ts*/}
      <p>Change password here</p>
    </ProfileSection>
  );
}
