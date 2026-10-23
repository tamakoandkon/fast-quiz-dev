<template>
  <UDrawer v-model:open="open" direction="right" :modal="false">
    <template #content>
      <div class="answer-sheet">
        <div class="answer-sheet-heading">
          <h2>答题卡</h2>
          <button type="button" aria-label="关闭答题卡" @click="open = false"><Icon name="i-heroicons-x-mark" /></button>
        </div>
        <div class="answer-sheet-grid">
          <button
            v-for="item in numberedIds"
            :key="item.id"
            @click="emit('click', item.id)"
            type="button"
            class="answer-sheet-item"
            :class="{ 'is-answered': answeredIds?.includes(item.id) }"
            :aria-label="`第 ${item.number} 题，${answeredIds?.includes(item.id) ? '已答' : '未答'}`"
          >
            {{ item.number }}
          </button>
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
  answeredIds?: string[];
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
