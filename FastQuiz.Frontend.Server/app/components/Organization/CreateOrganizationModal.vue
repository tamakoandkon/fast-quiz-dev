<template>
  <UModal title="新增组织" v-model:open="show">
    <UButton label="新增组织" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="name"></CommonFormInput>

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="createOrg"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>
<script setup lang="ts">
const show = defineModel({ required: true, default: true });
const name = ref<string>("");

const props = defineProps({
  parentId: {
    type: String,
    default: undefined,
  },
});

const emit = defineEmits<{
  (e: "create", payLoad: CreateOrganizationCommand): void;
}>();

const createOrg = function () {
  emit("create", { name: name.value, parentId: props.parentId });
  name.value = "";
};
</script>

<style scoped></style>
