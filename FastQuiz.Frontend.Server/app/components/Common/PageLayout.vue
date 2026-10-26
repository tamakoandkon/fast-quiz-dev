<template>
  <div class="page-shell">
    <CommonPageHeading :title="title">
      <template v-if="$slots.actions" #actions>
        <slot name="actions"></slot>
      </template>
    </CommonPageHeading>

    <div v-if="$slots.breadcrumb" class="page-breadcrumb">
      <slot name="breadcrumb"></slot>
      <USeparator />
    </div>

    <div v-if="$slots.header">
      <slot name="header"></slot>
    </div>

    <!-- 内容区域 -->
    <div class="flex-1 min-w-0">
      <slot></slot>
    </div>

    <!-- 底部区域（可选） -->
    <div v-if="$slots.footer">
      <slot name="footer"></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps({
  // 覆盖路由推导出来的标题，供概览这类需要动态文案的页面使用。
  title: { type: String, default: undefined },
});
</script>
