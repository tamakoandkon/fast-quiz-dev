<template>
  <UModal title="导出成绩" description="导出考试记录为 CSV" v-model:open="show">
    <template #body>
      <div class="space-y-6">
        <UFormField label="考试模板">
          <USelect
            v-model="value.templateId"
            :items="templateOptions"
            value-key="id"
            placeholder="全部考试"
            class="w-full"
          />
        </UFormField>

        <UFormField label="是否只取最好的一次成绩">
          <div class="flex items-center gap-3">
            <USwitch v-model="value.onlyBest" color="primary" size="md" />
            <span class="text-sm text-gray-500">
              {{ value.onlyBest ? "每场考试只导出最高分" : "导出全部考试记录" }}
            </span>
          </div>
        </UFormField>

        <div
          class="rounded-lg px-4 py-3 text-sm text-gray-500 bg-gray-50 border border-gray-200 flex items-start gap-2"
        >
          <UIcon name="i-lucide-info" class="mt-0.5 shrink-0" />
          <span>
            导出范围为该考生的全部记录，将自动翻页拉取。此处的筛选与列表页的当前筛选相互独立。
          </span>
        </div>

        <div class="flex gap-3 justify-center">
          <UButton class="flex-1 justify-center" :loading="loading" @click="confirm">
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

const props = defineProps<{
  // 该考生参加过的考试（由父页面全量拉取并去重后传入）
  templates: { id: string; label: string }[];
}>();

// 导出用的独立筛选状态：每次打开都重置为默认值，不复用列表页筛选。
export interface ExportUserRecordsFilter {
  templateId?: string;
  onlyBest: boolean;
}

const value = ref<ExportUserRecordsFilter>({
  templateId: undefined,
  onlyBest: false,
});

// 选项首位「全部考试」（id 为 undefined），其后为各场考试。
const templateOptions = computed(() => [
  { id: undefined, label: "全部考试" },
  ...props.templates,
]);

watch(show, (open) => {
  if (open) {
    value.value = { templateId: undefined, onlyBest: false };
  }
});

const emit = defineEmits<{
  (e: "export", filter: ExportUserRecordsFilter): void;
}>();

const confirm = () => {
  emit("export", { ...value.value });
};
</script>

<style scoped></style>
