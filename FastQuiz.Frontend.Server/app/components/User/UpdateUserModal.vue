<template>
  <UModal title="修改用户" v-model:open="show">
    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="username"></CommonFormInput>
        <CommonFormInput label="邮箱" v-model="email"></CommonFormInput>
        <UFormField label="密码" class="w-full">
          <UInput v-model="password" type="password"></UInput>
        </UFormField>

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="updateUser"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>
<script setup lang="ts">
const { $api } = useNuxtApp();
const toast = useToast();
const show = defineModel({ required: true, default: true });
const username = ref<string>("");
const password = ref<string>("");
const email = ref<string>("");

onMounted(async () => {
  await init(props.id);
});

const emit = defineEmits<{
  (e: "update", payLoad: UpdateUserCommand): void;
}>();

const props = defineProps({
  id: {
    type: String,
    default: "",
  },
  tenantId: {
    type: String,
    default: "",
  },
});

watch(
  () => props.id,
  async (newId) => {
    if (newId) {
      await init(newId);
    }
  },
  { immediate: true },
);

const updateUser = function () {
  emit("update", {
    username: username.value,
    password: password.value,
    email: email.value,
  });
  username.value = "";
  password.value = "";
  email.value = "";
};

const init = async (id: string) => {
  if (!id || id == "") {
    return;
  }

  let result;
  try {
    result = await $api.tenant.userDetail(props.tenantId, id);
  } catch (e: any) {
    const title = e.response.data.title;
    toast.add({ title: "获取用户失败：" + title });
    return;
  }
  username.value = result.data.username ?? "";
  password.value = "";
  email.value = ""; // TODO
};
</script>

<style scoped></style>
