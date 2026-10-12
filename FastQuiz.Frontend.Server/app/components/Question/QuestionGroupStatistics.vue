<template>
  <UCard>
    <template #header>
      <div class="flex items-center justify-between gap-4">
        <p class="text-sm font-semibold text-gray-900">题目统计</p>
        <p v-if="loading" class="text-xs text-gray-400">统计中...</p>
      </div>
    </template>

    <div v-if="loading" class="py-16 text-center text-sm text-gray-400">
      正在加载题目统计...
    </div>

    <div v-else-if="errorMessage" class="py-16 text-center text-sm text-red-500">
      {{ errorMessage }}
    </div>

    <div
      v-else-if="totalCount === 0"
      class="py-16 text-center text-sm text-gray-400"
    >
      该题库暂无题目
    </div>

    <div v-else>
      <!-- 1. 题目总数 -->
      <div class="flex items-baseline gap-2 px-1">
        <span class="text-sm text-gray-500">题目总数</span>
        <span class="text-3xl font-semibold text-gray-900">{{ totalCount }}</span>
        <span class="text-sm text-gray-400">道</span>
      </div>

      <!-- 2 & 3. 题型分布 / 难度分布 -->
      <div class="mt-2 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <VChart :style="chartStyle" :option="typeOption" autoresize />
        <VChart :style="chartStyle" :option="levelOption" autoresize />
      </div>
    </div>
  </UCard>
</template>

<script setup lang="ts">
// vue-echarts：按需引入，避免整包 echarts 打进产物
import { use } from "echarts/core";
import { CanvasRenderer } from "echarts/renderers";
import { PieChart } from "echarts/charts";
import { LegendComponent, TitleComponent, TooltipComponent } from "echarts/components";
import VChart from "vue-echarts";

use([CanvasRenderer, PieChart, TitleComponent, TooltipComponent, LegendComponent]);

const props = defineProps({
  tenantId: {
    type: String,
    default: "",
  },
  groupId: {
    type: String,
    default: "",
  },
});

const { $api } = useNuxtApp();
const toast = useToast();

const loading = ref(true);
const errorMessage = ref("");
const totalCount = ref(0);
const types = ref<QuestionTypeCountDto[]>([]);
const levels = ref<QuestionLevelCountDto[]>([]);

// 与饼图扇区对应的配色，题型/难度两张图共用
const PIE_COLORS = [
  "#3b82f6",
  "#22c55e",
  "#f59e0b",
  "#a855f7",
  "#ef4444",
  "#14b8a6",
  "#6366f1",
  "#ec4899",
];

const typeLabel = (type?: QuestionType) => getQuestionTypeName(type as QuestionType);

/**
 * 显式给出图表高度：外层 CommonPageLayout 的内容区是 flex-1 + min-h-0，
 * 继承高度可能为 0，导致 echarts 量不到尺寸而整块空白
 * （console 会报 "Can't get DOM width or height"）。所以这里不依赖父级高度。
 */
const CHART_HEIGHT = 300;
const chartStyle = {
  width: "100%",
  height: `${CHART_HEIGHT}px`,
  minHeight: `${CHART_HEIGHT}px`,
};

/**
 * 难度直接按数据库中的数字展示。
 * level 0 是库里真实存在的一级难度（并非“未设置”），所以照数字显示。
 */
const levelLabel = (level?: number) => `难度 ${level ?? 0}`;

const buildPieOption = (
  title: string,
  data: { name: string; value: number }[],
): any => ({
  title: {
    text: title,
    left: "center",
    top: 0,
    textStyle: { fontSize: 14, fontWeight: 600, color: "#111827" },
  },
  tooltip: {
    trigger: "item",
    formatter: "{b}：{c} 道 ({d}%)",
  },
  legend: {
    bottom: 0,
    left: "center",
    icon: "circle",
    itemWidth: 8,
    itemHeight: 8,
    textStyle: { fontSize: 12, color: "#4b5563" },
    // 同时给出数量和占比说明
    formatter: (name: string) => {
      const item = data.find((d) => d.name === name);
      if (!item) return name;
      const percent = totalCount.value
        ? ((item.value / totalCount.value) * 100).toFixed(1)
        : "0.0";
      return `${name} ${item.value} 道 (${percent}%)`;
    },
  },
  color: PIE_COLORS,
  series: [
    {
      type: "pie",
      radius: ["42%", "68%"],
      center: ["50%", "48%"],
      avoidLabelOverlap: true,
      itemStyle: { borderColor: "#fff", borderWidth: 2 },
      label: { show: true, formatter: "{b}\n{d}%", fontSize: 12, color: "#374151" },
      labelLine: { length: 10, length2: 10 },
      data,
    },
  ],
});

const typeOption = computed(() =>
  buildPieOption(
    "题型分布",
    types.value.map((t) => ({
      name: typeLabel(t.type),
      value: t.count ?? 0,
    })),
  ),
);

const levelOption = computed(() =>
  buildPieOption(
    "难度分布",
    levels.value.map((l) => ({
      name: levelLabel(l.level),
      value: l.count ?? 0,
    })),
  ),
);

const fetchStatistics = async () => {
  if (!props.tenantId || !props.groupId) {
    return;
  }

  loading.value = true;
  errorMessage.value = "";

  try {
    const result = await $api.tenant.questionGroupStatisticsList(
      props.tenantId,
      props.groupId,
    );

    totalCount.value = result.data.totalCount ?? 0;
    // 过滤掉数量为 0 的分项，饼图不画空扇区
    types.value = (result.data.types ?? []).filter((t) => (t.count ?? 0) > 0);
    levels.value = (result.data.levels ?? []).filter((l) => (l.count ?? 0) > 0);
  } catch (e) {
    const error = e as any;
    const title = error?.response?.data?.title;
    errorMessage.value = "获取题目统计失败" + (title ? "：" + title : "");
    toast.add({ title: errorMessage.value, color: "error" });
  } finally {
    loading.value = false;
  }
};

watch(
  () => [props.tenantId, props.groupId],
  ([tenantId, groupId]) => {
    if (tenantId && groupId) {
      fetchStatistics();
    }
  },
  { immediate: true },
);

// 供父页面在题目增删改后刷新统计
defineExpose({ refresh: fetchStatistics });
</script>

<style scoped></style>
