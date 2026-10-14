<template>
  <div
    class="data-table w-full"
    :class="{ 'data-table-compact': compact }"
  >
    <table class="min-w-full divide-y divide-gray-200 table-fixed">
      <colgroup>
        <col v-if="selectable" style="width: 48px" />
        <col
          v-for="col in columns"
          :key="col.key"
          :style="{ width: col.width }"
        />
      </colgroup>
      <thead class="bg-gray-50">
        <tr>
          <th v-if="selectable" class="w-12 px-6 py-3 border-b text-center">
            <input
              type="checkbox"
              :checked="isAllSelected"
              :indeterminate="isIndeterminate"
              @change="toggleAll"
              class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
          </th>

          <th
            v-for="(col, index) in columns"
            :key="col.key || index"
            :style="{ textAlign: 'center' }"
            :class="[
              'px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider border-b',
              col.headerClass,
            ]"
          >
            {{ col.label }}
          </th>
        </tr>
      </thead>

      <tbody class="divide-y divide-gray-200">
        <tr
          v-for="(row, rowIndex) in data"
          :key="rowIndex"
          :class="[
            'hover:bg-gray-50 transition-colors',
            isSelected(row) ? 'bg-blue-50/30' : '',
          ]"
          @dblclick="handleRowDblClick(row, rowIndex)"
          @contextmenu="() => handleRowRightClick(row, rowIndex)"
        >
          <td v-if="selectable" class="px-6 py-4 text-center">
            <input
              type="checkbox"
              :checked="isSelected(row)"
              @change="toggleRow(row)"
              class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
          </td>

          <td
            v-for="(col, colIndex) in columns"
            :key="col.key || colIndex"
            :class="compact ? 'px-2 py-3 text-sm text-gray-600 text-center whitespace-normal' : 'px-6 py-4 text-sm text-gray-600 text-center whitespace-nowrap'"
          >
            <div class="flex min-w-0 justify-center items-center">
              <slot :name="col.key" :row="row" :index="rowIndex">
                <template v-if="col.getValue">
                  {{ col.getValue(row) }}
                </template>
                <template v-else-if="col.key">
                  {{ (row as any)[col.key] }}
                </template>
              </slot>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div v-if="!data.length" class="data-table-empty">
      <UIcon name="i-lucide-inbox" class="text-3xl text-dimmed" />
      <p>暂无数据，添加内容后将在这里显示。</p>
    </div>
  </div>
</template>

<script setup lang="ts" generic="T">
import { computed } from "vue";

export interface TableColumn<T> {
  key?: string;
  label: string;
  width?: string;
  headerClass?: string;
  getValue?: (row: T) => any;
}

const props = defineProps<{
  columns: TableColumn<T>[];
  data: T[];
  selectable?: boolean; // 是否开启选择功能
  compact?: boolean;
}>();

const selected = defineModel<T[]>();

// 判断是否被选中（基于 ID，类似于 C# 中的 .Any(x => x.Id == row.Id)）
const isSelected = (row: T) => {
  return selected.value?.find((x) => x == row) != null;
};

// 全选状态：选中的数量等于当前页数据量
const isAllSelected = computed(() => {
  return props.data.length > 0 && selected.value?.length === props.data.length;
});

// 半选状态（Indeterminate）：选中了一些但不是全部
const isIndeterminate = computed(() => {
  const selectedCount = selected.value?.length ?? 0;
  return selectedCount > 0 && selectedCount < props.data.length;
});

// 切换单行选中
const toggleRow = (row: T) => {
  const currentSelection = [...(selected.value || [])];
  const index = currentSelection.findIndex((x) => x == row); //currentSelection.findIndex((item) => item.id === row.id);

  if (index > -1) {
    currentSelection.splice(index, 1);
  } else {
    currentSelection.push(row);
  }

  selected.value = currentSelection;
};

// 切换全选
const toggleAll = () => {
  if (isAllSelected.value) {
    selected.value = [];
  } else {
    selected.value = [...props.data];
  }
};

// 定义双击事件
const emit = defineEmits<{
  rowDblClick: [row: T, index: number];
  rowRightClick: [row: T, index: number];
}>();

// 处理行双击
const handleRowDblClick = (row: T, index: number) => {
  emit("rowDblClick", row, index);
};

const handleRowRightClick = (row: T, index: number) => {
  emit("rowRightClick", row, index);
};
</script>

<style scoped>
.data-table-compact table { width: 100%; min-width: 580px; }
.data-table-compact th { padding: 12px 8px; }
</style>
