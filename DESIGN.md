# FastQuiz design system

## Product intent

FastQuiz helps learners find an exam, answer confidently, and understand the result. Organizers publish exams and manage people in the same workspace. Each screen should make the next useful action obvious without telling users what the layout already communicates.

This document adapts the `shape → distill → audit → polish` workflow from [Impeccable](https://github.com/pbakaus/impeccable) and the durable `DESIGN.md` approach collected by [Awesome DESIGN.md](https://github.com/VoltAgent/awesome-design-md). The visual language belongs to FastQuiz; it does not copy another product's assets or brand.

## Principles

- One primary action per task area. Keep alternatives available as quiet text or outline actions.
- Show only facts that change a decision: availability, duration, progress, answer state, result.
- Avoid secondary labels, slogans, explanatory card paragraphs, and icon tiles when a title or layout already explains the destination.
- Use familiar exam patterns: question number, option letter, progress, answer sheet, time remaining, submit.
- Make selection, saving, failure, and completion perceivable without relying on color alone.
- Preserve control: users can revisit questions, change answers, inspect the answer sheet, and cancel submission.

## Visual language

- Canvas `#f7f6f2`; sidebar `#faf9f5`; card `#fdfcfa`; input and peak surface `#ffffff`. Adjacent
  levels differ by under 2% luminance, so hierarchy comes from hairlines and soft shadow rather than
  from hard jumps between white and off-white.
- Hairlines `#ebe9e2`, `#e4e1d9`, `#d7d3c9` — pick the lightest one that still separates the surfaces.
  Never outline a card with a line darker than `#d7d3c9` unless it is the primary action.
- Ink: titles `#1d1f22`, body `#3c3f45`, secondary `#63666d`, hints `#8b8e94`.
- Accent: clear blue `#3158d9`, reserved for the primary action, selected answer, active navigation,
  and focus. Tinted surfaces use `#eef1fd` with a `#c9d3f5` edge.
- Focus never draws a hard offset ring. Focused inputs keep their radius and deepen in place with a
  two-layer shadow — `0 0 0 1px` accent at 20% hugging the edge, `0 0 0 4px` warm neutral at 8% just
  outside it, plus a 1px inset warm shadow. Buttons and links use the same pair at 3px. The transition
  runs 220ms; focus must read as the control settling, not as a light switching on.
- Typography: system sans stack optimized for Chinese, with compact headings and generous line height
  for question content. Page titles are 28px everywhere; overview greets at the same size so the
  baseline does not move between first- and second-level pages.
- Radius: 12–16px on functional surfaces, 8–10px on controls. Avoid nested cards and decorative gradients.
- Motion: subtle hover/focus feedback only; respect reduced motion.

## Page rhythm

- Every page uses one of two shells: `CommonPageLayout` or `CommonManagePageContainer`. Both render the
  same heading block and the same 20px gap, so moving between overview, exam listing, my exams, and the
  management pages keeps the title baseline and the first content row at identical coordinates.
- The heading block reserves a fixed height even for short titles. Do not add page-level padding on top
  of the shell — that is what makes navigation feel like it jumps.
- The document reserves the scrollbar gutter (`scrollbar-gutter: stable`), so pages that grow past one
  screen do not shift horizontally when the bar appears.
- Loading placeholders occupy the same block as the loaded list. Never swap a one-line message for a
  multi-row table without reserving height first.

## Collection lists

Every list of entities — exams, papers, question banks — uses the same `data-list` primitive instead of a
wide table. A table makes the reader scan twelve columns to answer one question; a list row answers it in
one line and keeps the actions where the eye already is.

- Row anatomy, left to right: identity (title, optional one-line note), one status or count, one secondary
  owner field, then actions. Nothing else earns a column. IDs, timestamps, and system fields belong in the
  detail page.
- Column widths live in one custom property (`--list-cols`) so a page can retune its rhythm without
  copying layout rules. The header row repeats the same tracks, which keeps labels and cells on one grid.
- Counts use tabular numerals and sit at 15px/600; metadata stays at 12–13px and `--ink-muted`. A row must
  have exactly one visually dominant line.
- Actions: one ghost text button for the routine verb, one kebab for the rest. Never two filled buttons in
  the same row.
- Selection is a mode, not a permanent column: the checkbox lives inline with the title, and selecting
  anything raises a contextual bar above the list with the count, the destructive action, and an escape.
- Pagination lives in `CommonListFooter`: page index, page size, prev/next. Cursor pagination has no total,
  so say "本页 N 条" instead of inventing a count.

## Progressive loading

- Never render a full-width "load more" button. It competes with the primary action and reads as a dead end.
- `CommonListSentinel` watches the end of the list and requests the next page when it approaches the
  viewport. It reserves 56px, so appending a page does not shift what the reader is looking at.
- While appending, the sentinel shows a pulsing dot and "正在加载"; once exhausted it becomes a centred
  hairline with "已显示全部 N 份考试". An end state must be stated, not implied by a disappearing button.
- First load uses skeleton rows shaped like the real row, never a text placeholder that collapses into
  content.

## Responsive

- Below 1260px the grid stops being a table: `data-list-row` becomes a two-row card (`main`/`status` over
  `owner`/`actions`) with the header hidden. Below 640px titles clamp to two lines and the footer stacks.
- Page heading actions wrap under the title before they ever compress the title itself.

## Page patterns

- Authentication: brand, task title, fields, primary submit, alternate route. No marketing copy in the form.
- Workspace: persistent navigation, a clear page heading, direct destinations. Keep context chrome quiet.
- Exam listing: each exam is a single row with name, timing, availability, and actions. Details stay on demand.
- Answering: a compact task header with progress and remaining time; question content occupies the main reading column; option rows are full-size semantic controls; navigation and submit stay predictable.
- Empty and error states: state what happened and offer an action only when the user can fix it.

## Review checklist

- Is any label or paragraph repeating a nearby heading or action?
- Can all interactive controls be reached and understood with a keyboard?
- Does the layout work at 320px and at desktop widths without hiding the primary action?
- Is answer state evident in both the question and answer sheet?
- Do long Chinese/English names wrap without clipping the action?
