<template>
  <UModal title="修改题库" v-model:open="show">
    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="_name"></CommonFormInput>
        <CommonFormInput label="备注" v-model="_remark"></CommonFormInput>

        <UButton
          class="w-full justify-center"
          label="确认"
          color="primary"
          variant="subtle"
          @click="updateGroup"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
const toast = useToast();

const group = ref<QuestionGroupDto>({});

const show = defineModel({ required: true, default: true });
const _name = ref("");
const _remark = ref("");

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

const emit = defineEmits<{
  (e: "update", payLoad: UpdateQuestionGroupCommand): void;
}>();

// 监听 id 变化，触发 fetchGroup
watch(
  () => props.id,
  async (newId) => {
    if (newId) {
      await fetchGroup(newId);
    }
  },
  { immediate: true },
);

const updateGroup = function () {
  emit("update", { name: _name.value, remark: _remark.value });
};

const fetchGroup = async (id: string) => {
  if (!id || id.trim() === "") {
    return;
  }

  let result;
  try {
    result = await $api.tenant.questionGroupDetail(props.tenantId, props.id);
    group.value = result.data;
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取题库失败：" + title, color: "error" });
    return;
  }

  _name.value = group.value.name as string;
  _remark.value = group.value.remark as string;
};
</script>

<style scoped></style>
