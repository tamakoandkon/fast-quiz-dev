<template>
  <UModal title="新增试卷样本" v-model:open="show">
    <UButton label="新增试卷样本" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <UFormField label="试卷样本数量" class="w-full">
          <UInputNumber
            v-model="count"
            :min="1"
            :max="100"
            class="w-full"
            increment-icon="iconoir:arrow-right"
            decrement-icon="iconoir:arrow-left"
          />
        </UFormField>

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="createQuizSnapshot"
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
});

const emit = defineEmits<{
  (
    e: "create",
    quizTemplateId: string,
    command: CreateQuizSnapshotCommand,
  ): void;
}>();

const createQuizSnapshot = function () {
  emit("create", props.quizTemplateId, { count: count.value });
  count.value = 1;
};
</script>

<style scoped></style>
