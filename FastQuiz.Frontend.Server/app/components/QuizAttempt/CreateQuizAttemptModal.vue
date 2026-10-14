<template>
  <UModal title="批量生成待领取试卷" v-model:open="show" :dismissible="!busy">
    <template #body>
      <CommonFormContainer>
        <p class="text-sm text-muted">按当前样本生成 1–100 份试卷，考生可在考试中心领取。</p>
        <UFormField label="试卷数量" required>
          <UInputNumber v-model="count" :min="1" :max="100" :step="1" :disabled="busy" class="w-full" />
        </UFormField>

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          :loading="busy"
          :disabled="!Number.isInteger(count) || count < 1 || count > 100"
          @click="createQuizAttempt"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>
<script setup lang="ts">
const show = defineModel({ required: true, default: false });
const count = ref<number>(1);

const props = defineProps({
  busy: { type: Boolean, default: false },
  quizTemplateId: {
    type: String,
    default: "",
  },
  quizSnapshotId: {
    type: String,
    default: "",
  },
});

const emit = defineEmits<{
  (
    e: "create",
    quizSnapshotId: string,
    command: CreateQuizAttemptCommand,
  ): void;
}>();

const createQuizAttempt = function () {
  if (props.busy || !Number.isInteger(count.value) || count.value < 1 || count.value > 100) return;
  emit("create", props.quizTemplateId, {
    count: count.value,
    quizSnapshotId: props.quizSnapshotId,
  });
};
</script>

<style scoped></style>
