import { useEffect, useState, type ChangeEvent } from 'react';
import { Button } from '@/shared/ui';
import styles from './AvatarUpload.module.css';

const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_SIZE_BYTES = 2 * 1024 * 1024;

interface AvatarUploadProps {
  avatarUrl?: string | null;
  fullName: string;
  onRemove?: () => void;
  removing?: boolean;
}

export function AvatarUpload({ avatarUrl, fullName, onRemove, removing }: AvatarUploadProps) {
  const [pending, setPending] = useState<{ file: File; url: string } | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const url = pending?.url;
    return () => {
      if (url) URL.revokeObjectURL(url);
    };
  }, [pending]);

  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    const selected = event.target.files?.[0];
    event.target.value = '';
    if (!selected) return;

    if (!ACCEPTED_TYPES.includes(selected.type)) {
      setError('Use a JPG, PNG or WebP image.');
      return;
    }
    if (selected.size > MAX_SIZE_BYTES) {
      setError('The image must be smaller than 2 MB.');
      return;
    }

    setError(null);
    setPending({ file: selected, url: URL.createObjectURL(selected) });
  }

  function handleCancel() {
    setPending(null);
    setError(null);
  }

  const shown = pending?.url ?? avatarUrl;

  return (
    <div className={styles.root}>
      <div className={styles.avatar} aria-hidden="true">
        {shown ? (
          <img src={shown} alt="" className={styles.image} />
        ) : (
          fullName.charAt(0).toUpperCase()
        )}
      </div>

      <div className={styles.controls}>
        <div className={styles.buttons}>
          <label className={styles.pick}>
            Change photo
            <input
              type="file"
              accept={ACCEPTED_TYPES.join(',')}
              className="visually-hidden"
              onChange={handleChange}
            />
          </label>

          {pending && (
            <>
              {/*Currently no file upload endpoint on the backend, so it only stores an image url.*/}
              <Button
                variant="primary"
                disabled
                title="Waiting for an upload endpoint on the backend"
              >
                Save photo
              </Button>
              <Button variant="secondary" onClick={handleCancel}>
                Cancel
              </Button>
            </>
          )}

          {!pending && avatarUrl && onRemove && (
            <Button variant="secondary" onClick={onRemove} disabled={removing}>
              {removing ? 'Removing…' : 'Remove photo'}
            </Button>
          )}
        </div>

        <p className={styles.hint}>
          {pending ? pending.file.name : 'JPG, PNG or WebP, up to 2 MB.'}
        </p>
        {error && (
          <p className={styles.error} role="alert">
            {error}
          </p>
        )}
      </div>
    </div>
  );
}
