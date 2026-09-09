<template>
  <UEditor
    v-model="content"
    content-type="markdown"
    class="border border-gray-300 rounded-md w-full"
    :placeholder="placeholder"
    v-slot="{ editor }"
  >
    <UEditorToolbar :editor="editor" :items="items" />
    <UEditorDragHandle :editor="editor" />
  </UEditor>
</template>

<script setup lang="ts">
import type { EditorToolbarItem } from "@nuxt/ui";
const props = defineProps<{ placeholder: string }>();
const content = defineModel({ required: true, default: "" });

const items: EditorToolbarItem[] = [
  { kind: "mark", mark: "bold", icon: "i-lucide-bold" },
  { kind: "mark", mark: "italic", icon: "i-lucide-italic" },
  { kind: "heading", level: 1, icon: "i-lucide-heading-1" },
  { kind: "heading", level: 2, icon: "i-lucide-heading-2" },
  { kind: "textAlign", align: "left", icon: "i-lucide-align-left" },
  { kind: "textAlign", align: "center", icon: "i-lucide-align-center" },
  { kind: "bulletList", icon: "i-lucide-list" },
  { kind: "orderedList", icon: "i-lucide-list-ordered" },
  { kind: "blockquote", icon: "i-lucide-quote" },
  { kind: "link", icon: "i-lucide-link" },
];
</script>

<style scoped></style>
