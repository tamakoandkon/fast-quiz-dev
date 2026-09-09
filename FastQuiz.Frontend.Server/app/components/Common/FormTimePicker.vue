<template>
  <div class="flex gap-2">
    <UInputDate v-model="date" type="date" size="sm" />
    <UInputTime v-model="time" type="time" size="sm" />
  </div>
</template>

<script setup lang="ts">
import { CalendarDate, Time } from "@internationalized/date";

const modelValue = defineModel<string>({
  // 设置默认值为当前 UTC 时间
  default: () => new Date().toISOString(),
});

const date = ref<CalendarDate>();
const time = ref<Time>();

// 从 UTC 字符串解析到本地时间显示
function parseUTCToLocal(utcString: string) {
  const parsed = new Date(utcString);
  if (isNaN(parsed.getTime())) return false;

  const localDate = new Date(parsed.getTime());
  date.value = new CalendarDate(
    localDate.getFullYear(),
    localDate.getMonth() + 1,
    localDate.getDate(),
  );
  time.value = new Time(
    localDate.getHours(),
    localDate.getMinutes(),
    localDate.getSeconds(),
  );
  return true;
}

// 从本地时间生成 UTC 字符串
function getUTCString(): string {
  if (!date.value || !time.value) {
    return "";
  }

  const localDate = new Date(
    date.value.year,
    date.value.month - 1,
    date.value.day,
    time.value.hour,
    time.value.minute,
    time.value.second || 0,
  );

  return localDate.toISOString();
}

// 初始化：从 modelValue 解析，如果无效则使用当前时间
function initializeFromModel() {
  if (modelValue.value) {
    const success = parseUTCToLocal(modelValue.value);
    if (success) return;
  }

  // 如果 modelValue 无效或为空，使用当前时间
  const now = new Date();
  date.value = new CalendarDate(
    now.getFullYear(),
    now.getMonth() + 1,
    now.getDate(),
  );
  time.value = new Time(now.getHours(), now.getMinutes(), now.getSeconds());

  // 更新 modelValue 为当前 UTC 时间
  modelValue.value = getUTCString();
}

// 用户修改时间 → 更新 modelValue
watch(
  [date, time],
  () => {
    if (!date.value || !time.value) {
      modelValue.value = "";
      return;
    }
    modelValue.value = getUTCString();
  },
  { deep: true },
);

// 外部修改 modelValue → 更新界面
watch(
  () => modelValue.value,
  (val) => {
    if (!val || val === "") {
      // 如果外部设为空，重置为当前时间
      const now = new Date();
      date.value = new CalendarDate(
        now.getFullYear(),
        now.getMonth() + 1,
        now.getDate(),
      );
      time.value = new Time(now.getHours(), now.getMinutes(), now.getSeconds());
      modelValue.value = getUTCString();
      return;
    }

    parseUTCToLocal(val);
  },
);

// 组件挂载时初始化
onMounted(() => {
  initializeFromModel();
  console.log(modelValue.value);
});
</script>
