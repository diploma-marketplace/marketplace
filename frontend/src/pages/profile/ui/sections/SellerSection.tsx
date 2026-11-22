import { ProfileSection } from './ProfileSection';

export function SellerSection() {
  return (
    <ProfileSection title="Selling">
      {/* TODO: seller verification (POST /api/users/me/verification),
            listings used vs limit, analytics (GET /api/users/me/analytics). Show only what the role/verification allows */}
      <p>Seller verification, listings and analytics.</p>
    </ProfileSection>
  );
}
