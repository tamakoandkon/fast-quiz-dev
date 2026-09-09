<template>
  <UModal title="试卷模板详情" v-model:open="show">
    <template #body>
      <div class="space-y-4 py-2">
        <!-- 信息行 -->
        <div class="flex border-b border-gray-100 pb-3">
          <span class="w-24 text-sm text-gray-500 flex-shrink-0">名称</span>
          <span class="text-sm text-gray-900">{{ name || "-" }}</span>
        </div>

        <div class="flex border-b border-gray-100 pb-3">
          <span class="w-24 text-sm text-gray-500 flex-shrink-0">备注</span>
          <span class="text-sm text-gray-900">{{ remark || "-" }}</span>
        </div>

        <div class="flex border-b border-gray-100 pb-3">
          <span class="w-24 text-sm text-gray-500 flex-shrink-0">详情</span>
          <CommonMarkUpViewer :content="description" />
        </div>

        <div class="flex border-b border-gray-100 pb-3">
          <span class="w-24 text-sm text-gray-500 flex-shrink-0">考试次数</span>
          <span class="text-sm text-gray-900">{{ attemptLimit }} 次</span>
        </div>

        <div class="flex border-b border-gray-100 pb-3">
          <span class="w-24 text-sm text-gray-500 flex-shrink-0">长期有效</span>
          <span class="text-sm text-gray-900">
            <UBadge
              :color="isUnlimitedTime ? 'primary' : 'info'"
              variant="subtle"
            >
              {{ isUnlimitedTime ? "是" : "否" }}
            </UBadge>
          </span>
        </div>

        <div class="flex border-b border-gray-100 pb-3">
          <span class="w-24 text-sm text-gray-500 flex-shrink-0">开始时间</span>
          <span class="text-sm text-gray-900">{{ formatTime(beginTime) }}</span>
        </div>

        <div class="flex border-b border-gray-100 pb-3">
          <span class="w-24 text-sm text-gray-500 flex-shrink-0">结束时间</span>
          <span class="text-sm text-gray-900">{{ formatTime(endTime) }}</span>
        </div>

        <div class="flex pb-1">
          <span class="w-24 text-sm text-gray-500 flex-shrink-0">考试时长</span>
          <span class="text-sm text-gray-900">{{ durationMinutes }} 分钟</span>
        </div>
      </div>

      <!-- 底部按钮 -->
      <div class="mt-6 pt-4 border-t border-gray-200">
        <UButton
          label="关闭"
          color="info"
          variant="subtle"
          @click="
            () => {
              show = false;
            }
          "
          class="w-full justify-center"
        />
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();

const show = defineModel({ required: true, default: true });

const name = ref<string>("");
const description = ref<string>("");
const remark = ref<string>("");
const attemptLimit = ref<number>(1);
const isUnlimitedTime = ref<boolean>(true);
const beginTime = ref<Date>(new Date());
const endTime = ref<Date>(new Date());
const durationMinutes = ref<number>(60);

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

// 格式化时间
const formatTime = (date: Date) => {
  if (!date) return "-";
  try {
    return new Date(date).toLocaleString("zh-CN", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      hour12: false,
    });
  } catch {
    return "-";
  }
};

// 监听 ID 变化拉取数据
watch(
  () => props.id,
  async (newId) => {
    if (newId && newId.trim() !== "") {
      await fetchQuizTemplate(newId);
    }
  },
  { immediate: true },
);

const fetchQuizTemplate = async (id: string) => {
  if (!id || id.trim() === "") return;

  try {
    const result = await $api.tenant.quizTemplateDetail(props.tenantId, id);

    name.value = result.data.name ?? "";
    description.value = result.data.description ?? "";
    remark.value = result.data.remark ?? "";
    attemptLimit.value = result.data.attemptLimit ?? 1;
    isUnlimitedTime.value = result.data.isUnlimitedTime ?? false;
    beginTime.value = result.data.beginTime
      ? new Date(result.data.beginTime)
      : new Date();
    endTime.value = result.data.endTime
      ? new Date(result.data.endTime)
      : new Date();
    durationMinutes.value = result.data.durationMinutes ?? 0;
  } catch (error) {
    console.error("获取试卷模板详情失败:", error);
  }
};

// 关闭弹窗时重置数据
watch(show, (newVal) => {
  if (!newVal) {
    name.value = "";
    description.value = "";
    remark.value = "";
    attemptLimit.value = 1;
    isUnlimitedTime.value = false;
    beginTime.value = new Date();
    endTime.value = new Date();
    durationMinutes.value = 60;
  }
});
</script>

<style scoped></style>
