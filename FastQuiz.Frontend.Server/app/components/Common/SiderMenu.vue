<template>
  <aside
    class="bg-gray-100 h-screen transition-all duration-300 flex flex-col relative"
    :class="collapsed ? 'w-[60px]' : 'w-[200px]'"
  >
    <!-- 收缩/展开按钮 -->
    <button
      class="absolute -right-3 top-4 w-6 h-6 bg-white rounded-full shadow-md flex items-center justify-center cursor-pointer hover:bg-gray-50 transition-colors"
      @click="toggleCollapse"
    >
      {{ collapsed ? "→" : "←" }}
    </button>

    <!-- 菜单项列表 -->
    <nav class="flex flex-col gap-2 p-2 mt-12">
      <div
        v-for="item in menuItems"
        :key="item.id"
        class="flex items-center gap-2 p-2 overflow-hidden whitespace-nowrap hover:bg-gray-200 rounded transition-colors"
      >
        <UIcon :name="item.icon"></UIcon>
        <NuxtLink :to="item.link" v-show="!collapsed" class="text-sm">{{
          item.title
        }}</NuxtLink>
      </div>
    </nav>
  </aside>
</template>

<script setup lang="ts">
export interface MenuItem {
  id: number | string;
  title: string;
  icon: string;
  link: string;
}

defineProps({
  menuItems: {
    default: [],
    type: Array<MenuItem>,
  },
});

const collapsed = defineModel({ type: Boolean });

const toggleCollapse = () => {
  collapsed.value = !collapsed.value;
};
</script>
