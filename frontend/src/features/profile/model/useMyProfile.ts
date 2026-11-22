import { useAuth } from '@/features/auth';
import { profileApi, type UpdateProfilePayload } from '@/features/profile/api/profileApi.ts';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

const profileKey = (userId: number | undefined) => ['profile', 'me', userId] as const;

export function useMyProfile() {
  const { session } = useAuth();
  const userId = session?.userId;

  return useQuery({
    queryKey: profileKey(userId),
    queryFn: ({ signal }) => profileApi.getMe(signal),
    enabled: userId !== undefined,
  });
}

export function useUpdateProfile() {
  const queryClient = useQueryClient();
  const { session, refreshSession } = useAuth();
  const userId = session?.userId;

  return useMutation({
    mutationFn: (payload: UpdateProfilePayload) => profileApi.updateMe(payload),
    onSuccess: async (saved) => {
      queryClient.setQueryData(profileKey(userId), saved);
      await refreshSession();
    },
  });
}
