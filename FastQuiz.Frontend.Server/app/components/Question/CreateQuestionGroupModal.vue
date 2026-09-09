<template>
  <UModal title="新增题库" v-model:open="show">
    <UButton label="新增题库" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="name"></CommonFormInput>
        <CommonFormInput label="备注" v-model="remark"></CommonFormInput>

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="createGroup"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>
<script setup lang="ts">
const show = defineModel({ required: true, default: true });
const name = ref<string>("");
const remark = ref<string>("");

const emit = defineEmits<{
  (e: "create", payLoad: CreateQuestionGroupCommand): void;
}>();

const createGroup = function () {
  emit("create", { name: name.value, remark: remark.value });
  name.value = "";
  remark.value = "";
};
</script>

<style scoped></style>
