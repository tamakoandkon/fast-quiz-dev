<template>
  <div class="inline-flex items-center gap-2 font-mono" role="timer" aria-label="剩余时间">
    <CommonTimeUnit :value="minutes" label="分" />
    <span aria-hidden="true">:</span>

    <CommonTimeUnit :value="seconds" label="秒" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";

const props = defineProps({
  minutes: {
    type: Number,
    required: true,
    validator: (val) => val > 0,
  },
  startTime: {
    type: [Number, String],
    required: true,
  },
});

const emit = defineEmits(["end"]);

const remainingMs = ref(0);
let timer = null;
let ended = false;

const calcRemaining = () => {
  const start =
    typeof props.startTime === "string"
      ? new Date(props.startTime).getTime()
      : props.startTime;

  const endTime = start + props.minutes * 60 * 1000;
  const remain = endTime - Date.now();

  remainingMs.value = Math.max(0, remain);

  if (remainingMs.value === 0 && !ended) {
    ended = true;
    clearInterval(timer);
    emit("end");
  }
};

const formatTime = (ms) => {
  const totalSeconds = Math.floor(ms / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return { minutes, seconds };
};

const time = computed(() => formatTime(remainingMs.value));
const minutes = computed(() => time.value.minutes);
const seconds = computed(() => time.value.seconds);

onMounted(() => {
  calcRemaining();
  if (!ended) timer = setInterval(calcRemaining, 1000);
});

onUnmounted(() => {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
});
</script>
