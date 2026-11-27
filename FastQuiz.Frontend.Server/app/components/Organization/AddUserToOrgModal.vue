<template>
  <UModal title="添加组织成员" v-model:open="show">
    <UButton label="添加成员" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <UFormField label="用户名" class="w-full">
          <USelect
            :disabled="userOptions.length === 0"
            :items="userOptions"
            value-key="id"
            :placeholder="
              userOptions.length === 0 ? '没有可添加的用户' : '选择用户'
            "
            v-model="uId"
            class="w-full"
          />
        </UFormField>

        <UButton
          label="添加"
          color="primary"
          variant="subtle"
          :disabled="!uId"
          @click="addUser"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>
<script setup lang="ts">
const show = defineModel({ required: true, default: true });

const props = defineProps<{
  oId: string;
  users: UserDto[];
}>();

const userOptions = computed<{ label: string; id: string }[]>(() => {
  return props.users.flatMap((user) => user.id
    ? [{ label: user.username || "未命名用户", id: user.id }]
    : []);
});

const uId = ref<string | undefined>(undefined);

const emit = defineEmits<{
  (e: "add", payLoad: { userId: string; orgId: string }): void;
}>();

const addUser = () => {
  if (!uId.value || uId.value == "") {
    return;
  }
  emit("add", { userId: uId.value, orgId: props.oId });
  uId.value = undefined;
};
</script>

<style scoped></style>
