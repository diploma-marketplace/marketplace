import { useId, useState, type KeyboardEvent } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useDebouncedValue } from '@/shared/lib';
import { fetchSuggestions } from '../api/suggest';
import styles from './SearchBar.module.css';

export function SearchBar() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const [value, setValue] = useState(params.get('q') ?? '');
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);
  const listId = useId();

  const debounced = useDebouncedValue(value.trim(), 250);
  const { data: suggestions = [] } = useQuery({
    queryKey: ['suggest', debounced],
    queryFn: ({ signal }) => fetchSuggestions(debounced, signal),
    enabled: debounced.length >= 2,
  });
  const show = open && debounced.length >= 2 && suggestions.length > 0;

  const submit = (text: string) => {
    const q = text.trim();
    setValue(q);
    setOpen(false);
    setActive(-1);
    navigate(q ? `/catalog?q=${encodeURIComponent(q)}` : '/catalog');
  };

  const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Escape') {
      setOpen(false);
      return;
    }
    if (!show) return;
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setActive((i) => (i + 1) % suggestions.length);
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setActive((i) => (i <= 0 ? suggestions.length - 1 : i - 1));
    } else if (e.key === 'Enter' && active >= 0) {
      e.preventDefault();
      submit(suggestions[active]);
    }
  };

  return (
    <form
      role="search"
      className={styles.form}
      onSubmit={(e) => {
        e.preventDefault();
        submit(value);
      }}
    >
      <input
        type="search"
        role="combobox"
        className={styles.input}
        placeholder="Search..."
        aria-label="Search"
        aria-expanded={show}
        aria-controls={listId}
        aria-autocomplete="list"
        aria-activedescendant={active >= 0 ? `${listId}-${active}` : undefined}
        autoComplete="off"
        value={value}
        onChange={(e) => {
          setValue(e.target.value);
          setOpen(true);
          setActive(-1);
        }}
        onFocus={() => setOpen(true)}
        onBlur={() => setOpen(false)}
        onKeyDown={onKeyDown}
      />
      {show && (
        <ul id={listId} role="listbox" className={styles.list}>
          {suggestions.map((s, i) => (
            <li
              key={s}
              id={`${listId}-${i}`}
              role="option"
              aria-selected={i === active}
              className={i === active ? `${styles.option} ${styles.optionActive}` : styles.option}
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => submit(s)}
            >
              {s}
            </li>
          ))}
        </ul>
      )}
    </form>
  );
}