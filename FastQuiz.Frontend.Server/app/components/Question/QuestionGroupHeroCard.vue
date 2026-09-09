<template>
  <UCard>
    <div class="flex flex-wrap sm:flex-nowrap gap-6">
      <div class="min-w-0 flex-1">
        <p class="text-sm text-gray-500 mb-1">题库名称</p>
        <p class="text-xl font-semibold text-gray-900 break-words">
          {{ group.name }}
        </p>
      </div>
      <div class="min-w-0 flex-[2]">
        <p class="text-sm text-gray-500 mb-1">备注</p>
        <p class="text-gray-600 break-words">{{ group?.remark || "无" }}</p>
      </div>
      <div class="flex-shrink-0">
        <p class="text-sm text-gray-500 mb-1">题目数量</p>
        <p class="text-xl font-semibold text-gray-900">{{ group?.count }}</p>
      </div>
    </div>
  </UCard>
</template>

<script setup lang="ts">
const group = defineProps({
  name: {
    type: String,
    default: "",
  },
  remark: {
    type: String,
    default: "",
  },
  count: {
    type: Number,
    default: 0,
  },
});
</script>

<style scoped></style>
