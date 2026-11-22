import { useState, type ChangeEvent, type FormEvent } from 'react';
import { Button, TextField } from '@/shared/ui';
import {
  mapProfileError,
  toContactsOverrides,
  toContactsValues,
  toUpdatePayload,
  useUpdateProfile,
  validateContacts,
  type ContactsErrors,
  type ContactsValues,
  type UserProfile,
} from '@/features/profile';
import styles from './ContactsForm.module.css';

export function ContactsForm({ profile }: { profile: UserProfile }) {
  const update = useUpdateProfile();
  const isCompany = profile.role === 'Company';

  const [values, setValues] = useState<ContactsValues>(() => toContactsValues(profile));
  const [errors, setErrors] = useState<ContactsErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  const setField = (key: keyof ContactsValues) => (event: ChangeEvent<HTMLInputElement>) => {
    setValues((prev) => ({ ...prev, [key]: event.target.value }));
    setSaved(false);
  };

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (update.isPending) return;

    const nextErrors = validateContacts(values, { isCompany });
    setErrors(nextErrors);
    setFormError(null);
    setSaved(false);
    if (Object.keys(nextErrors).length > 0) return;

    update.mutate(toUpdatePayload(profile, toContactsOverrides(values, { isCompany })), {
      onSuccess: (savedProfile) => {
        setValues(toContactsValues(savedProfile));
        setSaved(true);
      },
      onError: (error) => {
        const mapped = mapProfileError(error);
        setErrors(mapped.fieldErrors);
        setFormError(mapped.formError);
      },
    });
  }

  function handleReset() {
    setValues(toContactsValues(profile));
    setErrors({});
    setFormError(null);
    setSaved(false);
    update.reset();
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <TextField
        label="Full name"
        visibleLabel
        name="fullName"
        autoComplete="name"
        value={values.fullName}
        onChange={setField('fullName')}
        error={errors.fullName}
      />
      <TextField
        label="Email"
        visibleLabel
        type="email"
        name="email"
        value={profile.email}
        readOnly
        className={styles.readonly}
      />
      <TextField
        label="Phone"
        visibleLabel
        type="tel"
        name="phoneNumber"
        autoComplete="tel"
        placeholder="+380501234567"
        value={values.phoneNumber}
        onChange={setField('phoneNumber')}
        error={errors.phoneNumber}
      />
      <TextField
        label="Address"
        visibleLabel
        name="address"
        autoComplete="street-address"
        value={values.address}
        onChange={setField('address')}
        error={errors.address}
      />

      {isCompany && (
        <>
          <TextField
            label="Company name"
            visibleLabel
            name="companyName"
            autoComplete="organization"
            value={values.companyName}
            onChange={setField('companyName')}
            error={errors.companyName}
          />
          <TextField
            label="Website"
            visibleLabel
            type="url"
            name="websiteUrl"
            autoComplete="url"
            placeholder="https://"
            value={values.websiteUrl}
            onChange={setField('websiteUrl')}
            error={errors.websiteUrl}
          />
        </>
      )}

      <fieldset className={styles.group}>
        <legend className={styles.legend}>Social links</legend>
        <TextField
          label="Telegram"
          visibleLabel
          name="telegram"
          autoComplete="off"
          placeholder="@username or link"
          value={values.telegram}
          onChange={setField('telegram')}
          error={errors.telegram}
        />
        <TextField
          label="Instagram"
          visibleLabel
          name="instagram"
          autoComplete="off"
          placeholder="@username or link"
          value={values.instagram}
          onChange={setField('instagram')}
          error={errors.instagram}
        />
        <TextField
          label="Facebook"
          visibleLabel
          name="facebook"
          autoComplete="off"
          placeholder="Profile or page link"
          value={values.facebook}
          onChange={setField('facebook')}
          error={errors.facebook}
        />
      </fieldset>

      {formError && (
        <p className={styles.formError} role="alert">
          {formError}
        </p>
      )}
      {saved && (
        <p className={styles.saved} role="status">
          Changes saved.
        </p>
      )}

      <div className={styles.actions}>
        <Button variant="secondary" onClick={handleReset} disabled={update.isPending}>
          Reset
        </Button>
        <Button variant="primary" type="submit" disabled={update.isPending}>
          {update.isPending ? 'Saving…' : 'Save changes'}
        </Button>
      </div>
    </form>
  );
}
