export default defineAppConfig({
  ui: {
    colors: { primary: 'blue', neutral: 'zinc' },
    button: { slots: { base: 'rounded-xl font-medium transition-all duration-200' } },
    input: { slots: { base: 'rounded-xl' } },
    textarea: { slots: { base: 'rounded-xl' } },
    select: { slots: { base: 'rounded-xl' } },
    card: { slots: { root: 'rounded-2xl shadow-none ring-1 ring-black/5', header: 'border-default', footer: 'border-default' } },
    modal: { slots: { content: 'rounded-3xl', header: 'px-6 py-5', body: 'p-6' } },
    badge: { slots: { base: 'rounded-full' } },
  },
});
