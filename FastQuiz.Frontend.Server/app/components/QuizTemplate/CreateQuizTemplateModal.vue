<template>
  <UModal title="新增试卷模板" v-model:open="show">
    <UButton
      label="新增试卷模板"
      color="primary"
      variant="solid"
      icon="i-lucide-plus"
    />

    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="name"></CommonFormInput>
        <CommonFormInput label="备注" v-model="remark"></CommonFormInput>
        <UFormField label="详情" class="w-full">
          <CommonMarkUpEditor
            v-model="description"
            placeholder="输入详情内容..."
          />
        </UFormField>

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
const description = ref<string>("");
const remark = ref<string>("");
const attemptLimit = ref<number>(1);
const isUnlimitedTime = ref<boolean>(true);
const beginTime = ref<string>("");
const endTime = ref<string>("");
const durationMinutes = ref<number>(60);
const orgId = ref<string | undefined>(undefined);

const emit = defineEmits<{
  (e: "create", payLoad: CreateQuizTemplateCommand): void;
}>();

const props = defineProps<{
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

const createGroup = function () {
  emit("create", {
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
  name.value = "";
  description.value = "";
  remark.value = "";
  attemptLimit.value = 1;
  isUnlimitedTime.value = true;
  durationMinutes.value = 60;
  orgId.value = undefined;
};
</script>

<style scoped></style>
