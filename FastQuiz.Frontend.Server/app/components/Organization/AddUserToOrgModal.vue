<template>
  <UModal title="添加用户到组织" v-model:open="show">
    <UButton label="添加用户到组织" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <UFormField label="选择用户" class="w-full">
          <USelect
            :disabled="userOptions.length === 0"
            :items="userOptions"
            value-key="id"
            label="选择用户"
            :placeholder="
              userOptions.length === 0 ? '没有更多用户' : '选择用户'
            "
            v-model="uId"
            class="w-full"
          />
        </UFormField>

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
const { $api } = useNuxtApp();

const show = defineModel({ required: true, default: true });
const toast = useToast();

const props = defineProps<{
  oId: string;
  users: UserDto[];
}>();

const userOptions = computed<{ label: string; id: string }[]>(() => {
  return props.users.map((user) => ({
    label: (user.username || user.id) as string,
    id: user.id as string,
  }));
});

const uId = ref<string | undefined>(undefined);

const emit = defineEmits<{
  (e: "add", payLoad: { userId: string; orgId: string }): void;
}>();

const createOrg = async () => {
  if (!uId.value || uId.value == "") {
    return;
  }
  emit("add", { userId: uId.value, orgId: props.oId });
  uId.value = undefined;
};
</script>

<style scoped></style>
