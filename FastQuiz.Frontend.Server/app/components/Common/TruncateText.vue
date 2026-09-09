<template>
  <UTooltip :delay-duration="0" arrow :text="text">
    <div class="group relative inline-block">
      <div class="truncate pr-6" :style="{ maxWidth: width }" :dir="direction">
        <slot>{{ text }}</slot>
      </div>

      <UButton
        v-if="text"
        size="xs"
        color="neutral"
        variant="ghost"
        class="absolute right-0 top-1/2 -translate-y-1/2 opacity-0 group-hover:opacity-100 transition-opacity"
        :icon="copied ? 'i-lucide-check' : 'i-lucide-copy'"
        @click.stop="copyToClipboard"
      />
    </div>
  </UTooltip>
</template>

<script setup lang="ts">
interface Props {
  text: string;
  width?: string;
  direction?: "ltr" | "rtl";
}

const props = withDefaults(defineProps<Props>(), {
  width: "5rem",
  direction: "ltr",
});

const copied = ref(false);

const copyToClipboard = async () => {
  await navigator.clipboard.writeText(props.text);
  copied.value = true;
  setTimeout(() => (copied.value = false), 1500);
};
</script>
