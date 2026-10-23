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

- Canvas: warm off-white `#f7f6f2`; content surface: white; ink: `#202124`.
- Accent: clear blue `#3158d9`, reserved for the primary action, selected answer, active navigation, and focus.
- Dividers: `#e6e7e5`; secondary text: `#62666d`. Status colors need a text label.
- Typography: system sans stack optimized for Chinese, with compact headings and generous line height for question content.
- Radius: 12–16px on functional surfaces, 8–10px on controls. Avoid nested cards and decorative gradients.
- Motion: subtle hover/focus feedback only; respect reduced motion.

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
