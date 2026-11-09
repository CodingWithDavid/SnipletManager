# UI Design Style Guide (Raw HTML)

## Core Philosophy
Clean, "Corporate Minimalist" aesthetics using standard CSS utility classes.

## ⚠️ CSS Variables — Mandatory Rule

**NEVER hardcode hex colors, spacing values, or border radii directly in component CSS files.**

All design tokens must be defined once as CSS custom properties in `wwwroot/css/site.css` and referenced by name everywhere else. This means a single color change updates the entire application.

### Define tokens in `wwwroot/css/site.css`:
```css
:root {
  /* Colors */
  --color-primary:        #2563EB;
  --color-primary-hover:  #1D4ED8;
  --color-secondary:      #64748B;
  --color-secondary-hover:#475569;
  --color-error:          #EF4444;
  --color-success:        #22C55E;
  --color-text:           #1E293B;
  --color-background:     #F8FAFC;
  --color-border:         #CBD5E1;
  --color-white:          #FFFFFF;

  /* Spacing */
  --spacing-xs:   0.25rem;  /*  4px */
  --spacing-sm:   0.5rem;   /*  8px */
  --spacing-md:   1rem;     /* 16px */
  --spacing-lg:   1.5rem;   /* 24px */
  --spacing-xl:   2rem;     /* 32px */

  /* Shape */
  --radius-sm:    4px;
  --radius-md:    8px;
  --radius-lg:    12px;

  /* Typography */
  --font-size-sm: 0.875rem;
  --font-size-md: 1rem;
  --font-size-lg: 1.125rem;
}
```

### Reference variables in component CSS (`.razor.css`):
```css
/* ✅ CORRECT — uses design tokens */
.btn-primary {
  background-color: var(--color-primary);
  border-radius: var(--radius-md);
  padding: var(--spacing-sm) var(--spacing-md);
  color: var(--color-white);
}

/* ❌ WRONG — hardcoded values scatter magic numbers across the codebase */
.btn-primary {
  background-color: #2563EB;
  border-radius: 8px;
  padding: 0.5rem 1rem;
  color: #FFFFFF;
}
```

---

## Element Styles

Use the CSS variable names below — never the raw hex values.

1. **Inputs (`<input>`, `<select>`, `<textarea>`)**:
   - Class: `form-control`.
   - Focus State: border ring using `var(--color-primary)`.
   - Error State: border using `var(--color-error)` when invalid.

2. **Buttons (`<button>`)**:
   - **Primary**: background `var(--color-primary)`, color `var(--color-white)`, border-radius `var(--radius-md)`.
   - **Secondary**: background `var(--color-secondary)`, color `var(--color-white)`.
   - **Type**: Always specify `type="button"` or `type="submit"`.

3. **Data Grids (`<QuickGrid>`)**:
   - Apply class `table-striped` to the grid container.
   - Rows should highlight on hover using `var(--color-background)`.

4. **Layout**:
   - Use Flexbox `div` wrappers for form rows.
   - Spacing: `var(--spacing-md)` gap between input groups.
