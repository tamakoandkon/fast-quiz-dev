<template>
  <div class="inline-flex items-center gap-2 font-mono">
    <!-- 分 -->
    <CommonTimeUnit :value="minutes" label="分" />
    <span class="text-gray-400 text-lg">:</span>

    <!-- 秒 -->
    <CommonTimeUnit :value="seconds" label="秒" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";

const props = defineProps({
  // 倒计时总时长（分钟）
  minutes: {
    type: Number,
    required: true,
    validator: (val) => val > 0,
  },
  // 开始时间：时间戳（毫秒）或 ISO 字符串
  startTime: {
    type: [Number, String],
    required: true,
  },
});

const emit = defineEmits(["end"]);

// ---------- 状态 ----------
const remainingMs = ref(0);
let timer = null;

// ---------- 计算剩余时间 ----------
const calcRemaining = () => {
  const start =
    typeof props.startTime === "string"
      ? new Date(props.startTime).getTime()
      : props.startTime;

  const endTime = start + props.minutes * 60 * 1000;
  const remain = endTime - Date.now();

  remainingMs.value = Math.max(0, remain);

  if (remainingMs.value === 0) {
    clearInterval(timer);
    emit("end");
  }
};

// ---------- 格式化时间 ----------
const formatTime = (ms) => {
  const totalSeconds = Math.floor(ms / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return { minutes, seconds };
};

// ---------- 计算属性 ----------
const time = computed(() => formatTime(remainingMs.value));
const minutes = computed(() => time.value.minutes);
const seconds = computed(() => time.value.seconds);

// ---------- 生命周期 ----------
onMounted(() => {
  calcRemaining();
  timer = setInterval(calcRemaining, 1000);
});

onUnmounted(() => {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
});
</script>
