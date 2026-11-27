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

- The workspace pairs a navy navigation rail with a cool, light canvas and white surfaces. Blue marks the current section, focus, and primary actions. Secondary text uses slate, and status colors always accompany a text label.
- Use a 14px radius for content panels, 9–11px for supporting controls, thin blue-gray separators, and the system sans stack with Chinese fallbacks.
- Titles use 26px on desktop and 22px on narrow screens. Metadata uses 13px. Dates and scores use tabular numerals.

## Page rhythm

- Workspace pages share one centered content width. The top bar and page title start on the same horizontal line. All pages put the title and page actions in \`CommonPageLayout\`.
- \`workspaceLocation\` supplies the parent section, icon, and navigation trail. Detail pages keep the parent section selected and show the trail in the top bar. Put entity names or meaningful context in the page heading.
- Use the shared heading for details and results. Answering may use a narrower reading column for question content below that heading.

## Collection lists

- Collections use the same panel border, row rhythm, and action alignment. Use \`CommonDataTable\` for values that need comparison across columns. Use list rows when scanning by name is the main task.
- Lead with the name and an optional second line of metadata. Align row actions to the trailing edge. Keep filters above the results and pagination in \`CommonListFooter\`.
- On narrow screens, actions wrap in the heading and tables scroll within their panel. Use the filename or entity name in links and menu labels.

## Motion

- Keep navigation visible throughout the change. Fade in new page content with a small offset. Expand records and organizational groups in place, with the arrow showing the new state.
- Durations are 160–230ms. The reduced-motion media query shortens movement throughout the workspace.
- The implementation uses Vue transitions and CSS. The motion references were [Motion for Vue](https://motion.dev/docs/vue-layout-animations), [AutoAnimate](https://auto-animate.formkit.com/), [Anime.js](https://animejs.com/documentation/layout/), and [Reka UI](https://reka-ui.com/docs/components/accordion).

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
- Page heading actions wrap under the title while keeping a shared content width.

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
