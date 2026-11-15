import { useEffect, useState, type ReactNode } from 'react';
import styles from './Carousel.module.css';

interface CarouselProps {
    slides: ReactNode[];
    label: string;
    autoplayMs?: number;
}

export function Carousel({ slides, label, autoplayMs }: CarouselProps) {
    const [index, setIndex] = useState(0);
    const [paused, setPaused] = useState(false);
    const count = slides.length;
    const current = Math.min(index, Math.max(count - 1, 0));

    useEffect(() => {
        if (!autoplayMs || paused || count < 2) return;
        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

        const id = window.setInterval(() => setIndex((i) => (i + 1) % count), autoplayMs);
        return () => window.clearInterval(id);
    }, [autoplayMs, paused, count]);

    if (count === 0) return null;

    const goTo = (i: number) => setIndex((i + count) % count);

    return (
        <section
            className={styles.carousel}
            aria-roledescription="carousel"
            aria-label={label}
            onMouseEnter={() => setPaused(true)}
            onMouseLeave={() => setPaused(false)}
            onFocus={() => setPaused(true)}
            onBlur={() => setPaused(false)}
        >
            <div className={styles.viewport}>
                <div className={styles.track} style={{ transform: `translateX(-${current * 100}%)` }}>
                    {slides.map((slide, i) => (
                        <div
                            key={i}
                            className={styles.slide}
                            role="group"
                            aria-roledescription="slide"
                            aria-label={`${i + 1} of ${count}`}
                            inert={i !== current}
                        >
                            {slide}
                        </div>
                    ))}
                </div>
            </div>

            {count > 1 && (
                <>
                    <button
                        type="button"
                        className={`${styles.arrow} ${styles.prev}`}
                        aria-label="Previous slide"
                        onClick={() => goTo(current - 1)}
                    >
                        ‹
                    </button>
                    <button
                        type="button"
                        className={`${styles.arrow} ${styles.next}`}
                        aria-label="Next slide"
                        onClick={() => goTo(current + 1)}
                    >
                        ›
                    </button>
                    <div className={styles.dots}>
                        {slides.map((_, i) => (
                            <button
                                key={i}
                                type="button"
                                className={i === current ? `${styles.dot} ${styles.dotActive}` : styles.dot}
                                aria-label={`Go to slide ${i + 1}`}
                                aria-current={i === current ? 'true' : undefined}
                                onClick={() => goTo(i)}
                            />
                        ))}
                    </div>
                </>
            )}
        </section>
    );
}
