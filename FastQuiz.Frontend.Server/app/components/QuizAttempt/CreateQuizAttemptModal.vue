<template>
  <UModal title="新增试卷" v-model:open="show">
    <UButton label="新增试卷" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <CommonFormNumberInput v-model="count" lable="试卷数量" />

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
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
  emit("create", props.quizTemplateId, {
    count: count.value,
    quizSnapshotId: props.quizSnapshotId,
  });
  count.value = 1;
};
</script>

<style scoped></style>
