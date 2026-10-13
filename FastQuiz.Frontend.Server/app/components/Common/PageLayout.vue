<template>
  <div class="flex flex-col gap-5 h-full">
    <CommonPageHeading />
    <div v-if="$slots.breadcrumb">
      <slot name="breadcrumb"></slot>
      <USeparator class="my-2" />
    </div>

    <div v-if="$slots.header">
      <slot name="header"></slot>
    </div>

    <!-- 内容区域 -->
    <div class="flex-1 min-h-0">
      <slot></slot>
    </div>

    <!-- 底部区域（可选） -->
    <div v-if="$slots.footer">
      <slot name="footer"></slot>
    </div>
  </div>
</template>

<script setup lang="ts"></script>
