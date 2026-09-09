<template>
  <UModal title="修改试卷模板" v-model:open="show">
    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="name"></CommonFormInput>
        <CommonFormInput label="备注" v-model="remark"></CommonFormInput>
        <CommonMarkUpEditor
          v-model="description"
          placeholder="输入详情内容..."
        />
        <CommonFormNumberInput label="考试次数" v-model="attemptLimit">
        </CommonFormNumberInput>
        <CommonFormSwitch label="是否长期有效" v-model="isUnlimitedTime">
        </CommonFormSwitch>
        <div v-show="!isUnlimitedTime">
          <CommonFormTimePicker
            label="开始时间"
            v-model="beginTime"
          ></CommonFormTimePicker>
          <CommonFormTimePicker
            label="结束时间"
            v-model="endTime"
          ></CommonFormTimePicker>
        </div>
        <CommonFormNumberInput
          label="考试时长（分钟）"
          v-model="durationMinutes"
        >
        </CommonFormNumberInput>
        <UFormField label="限定组织" class="w-full">
          <USelect
            :items="orgOptions"
            value-key="id"
            label="限定组织"
            v-model="orgId"
            class="w-full"
          />
        </UFormField>
        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="updateQuizTemplate"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();

const show = defineModel({ required: true, default: true });

// 表单数据
const name = ref<string>("");
const description = ref<string>("");
const remark = ref<string>("");
const attemptLimit = ref<number>(1);
const isUnlimitedTime = ref<boolean>(true);
const beginTime = ref<string>("");
const endTime = ref<string>("");
const durationMinutes = ref<number>(60);
const orgId = ref<string | undefined>(undefined);

const props = defineProps<{
  id: string;
  tenantId: string;
  orgs: OrganizationDto[];
}>();

const orgOptions = computed(() => {
  return [
    {
      id: undefined,
      label: "全部组织",
    },
    ...props.orgs.map((org) => ({
      id: org.id,
      label: org.name ?? "未知组织",
    })),
  ];
});

const emit = defineEmits<{
  (e: "update", payLoad: UpdateQuizTemplateCommand): void;
}>();

// 重置表单
const resetForm = () => {
  name.value = "";
  description.value = "";
  remark.value = "";
  attemptLimit.value = 1;
  isUnlimitedTime.value = true;
  beginTime.value = "";
  endTime.value = "";
  durationMinutes.value = 60;
  orgId.value = undefined;
};

// 更新试卷模板
const updateQuizTemplate = function () {
  emit("update", {
    name: name.value,
    description: description.value,
    remark: remark.value,
    attemptLimit: attemptLimit.value,
    isUnlimitedTime: isUnlimitedTime.value,
    beginTime: beginTime.value,
    endTime: endTime.value,
    durationMinutes: durationMinutes.value,
    userGroupId: orgId.value,
  });
  resetForm();
};

// 获取试卷模板详情
const fetchQuizTemplate = async (id: string) => {
  if (!id || id.trim() === "") {
    return;
  }

  try {
    let result = await $api.tenant.quizTemplateDetail(props.tenantId, id);

    name.value = result.data.name ?? "";
    description.value = result.data.description ?? "";
    remark.value = result.data.remark ?? "";
    attemptLimit.value = result.data.attemptLimit ?? 1;
    isUnlimitedTime.value = result.data.isUnlimitedTime ?? true;
    beginTime.value = result.data.beginTime ?? "";
    endTime.value = result.data.endTime ?? "";
    durationMinutes.value = result.data.durationMinutes ?? 60;
    orgId.value = result.data.userGroupId ?? undefined;
  } catch (error) {
    console.error("获取试卷模板详情失败:", error);
  }
};

// 监听 ID 变化，加载数据
watch(
  () => props.id,
  async (newId) => {
    if (newId) {
      await fetchQuizTemplate(newId);
    }
  },
  { immediate: true },
);
</script>

<style scoped></style>
