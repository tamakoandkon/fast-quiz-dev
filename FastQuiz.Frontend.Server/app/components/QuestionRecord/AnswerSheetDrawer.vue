<template>
  <UDrawer v-model:open="open" direction="right" :modal="false">
    <template #content>
      <div class="p-4">
        <div class="flex flex-wrap gap-1.5 w-100">
          <div
            v-for="item in numberedIds"
            :key="item.id"
            @click="emit('click', item.id)"
            class="h-9 w-9 flex items-center justify-center bg-neutral-100 dark:bg-neutral-800 rounded cursor-pointer hover:bg-neutral-200 dark:hover:bg-neutral-700 text-sm select-none"
          >
            {{ item.number }}
          </div>
        </div>
      </div>
    </template>
  </UDrawer>
</template>

<script setup lang="ts">
import { computed } from "vue";

const open = defineModel({ required: true, default: false });

const props = defineProps<{
  ids: string[];
}>();

const emit = defineEmits<{
  (e: "click", id: string): void;
}>();

const numberedIds = computed(() => {
  return props.ids.map((id, index) => ({
    id,
    number: index + 1,
  }));
});
</script>
