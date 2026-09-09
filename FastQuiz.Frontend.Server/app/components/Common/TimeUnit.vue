<template>
  <div class="flex flex-col items-center">
    <div
      class="bg-gray-900 text-white rounded-lg px-3 py-2 min-w-[48px] text-center text-2xl font-bold shadow-lg"
    >
      {{ displayValue }}
    </div>
    <span class="text-xs text-gray-500 mt-1">{{ label }}</span>
  </div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  value: {
    type: Number,
    required: true,
  },
  label: {
    type: String,
    required: true,
  },
});

const displayValue = computed(() => String(props.value).padStart(2, "0"));
</script>
