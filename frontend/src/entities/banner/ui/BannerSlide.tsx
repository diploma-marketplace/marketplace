import type {Banner} from '../model/types';
import styles from "./BannerSlide.module.css";

export function BannerSlide({banner}: {banner: Banner}){
    if (!banner.imageUrl){
        return <div className={styles.slide}/>
    }
    const image = <img className={styles.image} src={banner.imageUrl} alt={banner.title}/>

    return banner.href ? (
        <a className={styles.slide} href={banner.href}>{image}</a>
    ) : (
        <div className={styles.slide}>{image}</div>
    );
}