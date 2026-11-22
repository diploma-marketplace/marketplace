import { ProfileSection } from './ProfileSection';

export function OrdersSection() {
  return (
    <ProfileSection title="My orders">
      {/* TODO: list from GET /api/orders/my (status filter, page, pageSize), link each order to its details. Needs loading / empty / error states */}
      <p>Your orders will appear here.</p>
    </ProfileSection>
  );
}
