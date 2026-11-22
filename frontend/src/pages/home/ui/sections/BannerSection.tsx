import { Carousel } from '@/shared/ui';
import { BannerSlide, bannerSlots } from '@/entities/banner';

export function BannerSection() {
  return (
    <Carousel
      label="Promotions"
      slides={bannerSlots.map((banner) => (
        <BannerSlide key={banner.id} banner={banner} />
      ))}
    />
  );
}
