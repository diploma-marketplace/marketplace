import { Carousel } from '@/shared/ui';
import { BannerSlide, bannerSlots } from '@/entities/banner';
import { usePreferences } from '@/app/providers/PreferencesProvider';

export function BannerSection() {
  const { t } = usePreferences();

  return (
    <Carousel
      label={t('promotions')}
      slides={bannerSlots.map((banner) => (
        <BannerSlide key={banner.id} banner={banner} />
      ))}
    />
  );
}