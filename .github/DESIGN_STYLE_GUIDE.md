# DragonTail Design Style Guide

This guide captures the visual system used in this codebase so you can apply the same look and feel in other applications.

## 1. Brand Direction

- Visual character: clean enterprise UI with one strong brand accent.
- Accent strategy: neutral surfaces and typography, with red used to indicate action and focus.
- Motion strategy: subtle, fast transitions for hover, focus, and list entrance.
- Density: medium. Comfortable spacing, card-first layouts, and clear section hierarchy.

## 2. Source Of Truth In This Repo

Primary source files:

- app/globals.css
- app/layout.tsx
- components/ui/button.tsx
- components/ui/card.tsx
- components/ui/input.tsx
- components/component-library.tsx
- components/category-card.tsx
- components/component-card.tsx

Note: styles/globals.css appears to be a generic/older token file. The app currently imports app/globals.css from app/layout.tsx.

## 3. Core Design Tokens

Use semantic tokens, not raw colors, in application code.

### 3.1 Light Theme Tokens

```css
:root {
  --background: #ffffff;
  --foreground: #0a0a0a;
  --card: #ffffff;
  --card-foreground: #0a0a0a;
  --popover: #ffffff;
  --popover-foreground: #0a0a0a;

  --primary: #ee373d;
  --primary-foreground: #ffffff;

  --secondary: #f5f5f5;
  --secondary-foreground: #0a0a0a;

  --muted: #f5f5f5;
  --muted-foreground: #737373;

  --accent: #fef2f2;
  --accent-foreground: #0a0a0a;

  --destructive: #ef4444;
  --destructive-foreground: #ffffff;

  --border: #e5e5e5;
  --input: #e5e5e5;
  --ring: #ee373d;

  --radius: 0.5rem;

  --sidebar: #fafafa;
  --sidebar-foreground: #262626;
  --sidebar-primary: #ee373d;
  --sidebar-primary-foreground: #ffffff;
  --sidebar-accent: #fef2f2;
  --sidebar-accent-foreground: #0a0a0a;
  --sidebar-border: #e5e5e5;
  --sidebar-ring: #ee373d;
}
```

### 3.2 Dark Theme Tokens

```css
.dark {
  --background: #0a0a0a;
  --foreground: #fafafa;
  --card: #171717;
  --card-foreground: #fafafa;
  --popover: #171717;
  --popover-foreground: #fafafa;

  --primary: #f04449;
  --primary-foreground: #ffffff;

  --secondary: #262626;
  --secondary-foreground: #fafafa;

  --muted: #262626;
  --muted-foreground: #a3a3a3;

  --accent: #262626;
  --accent-foreground: #fafafa;

  --destructive: #dc2626;
  --destructive-foreground: #ffffff;

  --border: #262626;
  --input: #262626;
  --ring: #f04449;

  --sidebar: #0a0a0a;
  --sidebar-foreground: #d4d4d4;
  --sidebar-primary: #f04449;
  --sidebar-primary-foreground: #ffffff;
  --sidebar-accent: #1c1c1c;
  --sidebar-accent-foreground: #ffffff;
  --sidebar-border: #262626;
  --sidebar-ring: #f04449;
}
```

### 3.3 Typography Tokens

Primary font stack in app/globals.css:

- Inter, Inter Fallback, system-ui, -apple-system, sans-serif

Utility classes used repeatedly:

```css
.heading-1 { @apply font-semibold tracking-tight text-4xl md:text-5xl; }
.heading-2 { @apply font-semibold tracking-tight text-3xl md:text-4xl; }
.heading-3 { @apply font-semibold tracking-tight text-2xl md:text-3xl; }
.heading-4 { @apply font-semibold tracking-tight text-xl md:text-2xl; }
.heading-5 { @apply font-semibold text-sm; }

.body-large  { @apply text-lg; }
.body-normal { @apply text-base; }
.body-small  { @apply text-sm; }
```

## 4. Component Style Recipes

These recipes capture the dominant look of this library.

### 4.1 Buttons

- Primary CTA: red fill, white text, slight darken on hover.
- Secondary: light neutral fill.
- Outline: red border/text, fills red on hover.
- Focus ring: tokenized ring color.
- Radius: rounded-md.

Tailwind recipe:

```tsx
"inline-flex items-center justify-center gap-2 rounded-md text-sm font-medium transition-all disabled:pointer-events-none disabled:opacity-50 focus-visible:ring-[3px] focus-visible:ring-ring/50"
```

Primary variant:

```tsx
"bg-primary text-primary-foreground hover:bg-primary/90"
```

### 4.2 Cards

- White/neutral card surface.
- Soft border.
- Subtle shadow that increases on hover.
- Title color shifts to primary on hover in many showcase cards.
- Animated list reveal with slight upward motion.

Tailwind recipe:

```tsx
"bg-card text-card-foreground rounded-xl border shadow-sm hover:shadow-lg transition-shadow"
```

### 4.3 Inputs/Search

- Border and fill are token-driven.
- Search icon in-field is common pattern.
- Focus state uses border-ring + ring.
- Height baseline is 36px (h-9 in UI primitives).

### 4.4 Sidebar Navigation

- Fixed left rail, 16rem width.
- Neutral sidebar surface with subtle border.
- Active section text/icon frequently uses brand red.
- Nested accordions for category and component levels.

### 4.5 Data/Status Styling

- Secondary and muted text rely on text-muted-foreground.
- Badges are used heavily for state and metadata chips.
- Loading/empty states use muted icon + short explanatory copy.

## 5. Layout And Spacing System

Observed layout rhythm:

- App shell: fixed sidebar + scrolling content area.
- Main content offset: ml-64.
- Container width: max-w-7xl mx-auto.
- Section rhythm: space-y-8 (large), space-y-4/6 (internal groups).
- Grid rhythm for cards: gap-6.
- Breakpoints for card grids: 1 column mobile, 2 md, 3 lg.

Reference pattern:

```tsx
<div className="ml-64 flex flex-col min-h-screen">
  <div className="flex-1 px-8 py-6">
    <div className="max-w-7xl mx-auto space-y-8">
      {/* content */}
    </div>
  </div>
</div>
```

## 6. Motion And Interaction

Common motion values:

- Standard transition duration: 200ms to 300ms.
- Card entrance: animate-in fade-in slide-in-from-bottom-4 with staggered delay.
- Icon affordance: tiny horizontal shift on hover (translate-x-1).
- Accordion content: simple fade/slide animation.

Guideline:

- Prefer fast and subtle transitions.
- Reserve larger movement for onboarding/showcase surfaces.

## 7. Brand Assets And Usage

Logo variants in public assets support:

- Horizontal full color (default on light backgrounds)
- Horizontal black (single-color usage)
- Horizontal white / white text (dark backgrounds)
- Mark/icon full color, black, white

Use principle:

- Default to full-color logo on light backgrounds.
- Use white variants only on dark or photographic backgrounds.
- Keep generous clear space around marks and avoid decorative effects.

## 8. Copy-Paste Starter For New Apps

Add this as your baseline token and utility layer (adapt as needed):

```css
@import "tailwindcss";
@import "tw-animate-css";

:root {
  --background: #ffffff;
  --foreground: #0a0a0a;
  --card: #ffffff;
  --card-foreground: #0a0a0a;
  --primary: #ee373d;
  --primary-foreground: #ffffff;
  --muted: #f5f5f5;
  --muted-foreground: #737373;
  --border: #e5e5e5;
  --ring: #ee373d;
  --radius: 0.5rem;
}

@layer base {
  * { @apply border-border outline-ring/50; }
  body { @apply bg-background text-foreground font-sans antialiased; }
}

@layer components {
  .heading-1 { @apply font-semibold tracking-tight text-4xl md:text-5xl; }
  .heading-2 { @apply font-semibold tracking-tight text-3xl md:text-4xl; }
  .heading-3 { @apply font-semibold tracking-tight text-2xl md:text-3xl; }
  .body-large { @apply text-lg; }
  .body-normal { @apply text-base; }
  .body-small { @apply text-sm; }
}
```

## 9. Migration Checklist For Other Applications

1. Add semantic tokens first (do not hardcode raw colors in components).
2. Add typography utilities (heading and body classes).
3. Port base primitives before complex screens:
   - button
   - input
   - card
   - badge
4. Implement app shell layout (fixed sidebar + max-width content container).
5. Apply motion presets (200ms to 300ms, subtle hover/entrance).
6. Replace one-off hex values with semantic token classes:
   - bg-primary, text-primary, ring-ring, border-border, text-muted-foreground
7. Validate color contrast for text on primary and muted surfaces.

## 10. Anti-Patterns To Avoid

- Overusing primary red for large surfaces.
- Mixing multiple accent colors that compete with brand red.
- Using deep shadows or long animation durations.
- Ignoring muted text hierarchy (everything same contrast).
- Hardcoding hex values in feature components when token classes exist.

## 11. Optional: Blazor-Friendly Token Export

For non-React apps (including Blazor), keep identical CSS variable names and apply them at :root and .dark so visual parity remains high across stacks.

```css
:root {
  --primary: #ee373d;
  --primary-foreground: #ffffff;
  --background: #ffffff;
  --foreground: #0a0a0a;
  --muted: #f5f5f5;
  --muted-foreground: #737373;
  --border: #e5e5e5;
  --ring: #ee373d;
}
```

Then map your framework classes/components to these variables for buttons, cards, and form controls.
