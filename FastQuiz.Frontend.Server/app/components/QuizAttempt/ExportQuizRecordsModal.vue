<template>
  <UModal title="导出成绩" description="导出全部考试记录为 CSV" v-model:open="show">
    <template #body>
      <div class="space-y-6">
        <UFormField label="是否只取最好的一次成绩">
          <div class="flex items-center gap-3">
            <USwitch v-model="value.onlyBest" color="primary" size="md" />
            <span class="text-sm text-gray-500">
              {{ value.onlyBest ? "每个考生只导出最高分" : "导出全部考试记录" }}
            </span>
          </div>
        </UFormField>

        <div
          class="rounded-lg px-4 py-3 text-sm text-gray-500 bg-gray-50 border border-gray-200 flex items-start gap-2"
        >
          <UIcon name="i-lucide-info" class="mt-0.5 shrink-0" />
          <span>
            导出范围为本场考试的全部记录，将自动翻页拉取。此处的筛选与列表页的当前筛选相互独立。
          </span>
        </div>

        <div class="flex gap-3 justify-center">
          <UButton
            class="flex-1 justify-center"
            :loading="loading"
            @click="confirm"
          >
            导出
          </UButton>
          <UButton
            class="flex-1 justify-center"
            color="error"
            :disabled="loading"
            @click="show = false"
          >
            取消
          </UButton>
        </div>
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
const show = defineModel({ required: true, default: false });

const loading = defineModel<boolean>("loading", { default: false });

// 导出用的独立筛选状态：每次打开都重置为默认值，不复用列表页筛选。
export interface ExportRecordsFilter {
  onlyBest: boolean;
}

const value = ref<ExportRecordsFilter>({ onlyBest: false });

watch(show, (open) => {
  if (open) {
    value.value = { onlyBest: false };
  }
});

const emit = defineEmits<{
  (e: "export", filter: ExportRecordsFilter): void;
}>();

const confirm = () => {
  emit("export", { ...value.value });
};
</script>

<style scoped></style>
