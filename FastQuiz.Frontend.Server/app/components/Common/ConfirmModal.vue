<template>
  <UModal :title="title" :description="descript" v-model:open="show">
    <template #body>
      <div class="flex gap-3 justify-center">
        <UButton class="flex-1 justify-center" @click="confirm">确认</UButton>
        <UButton class="flex-1 justify-center" @click="cancel" color="error"
          >取消</UButton
        >
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
defineProps({
  title: {
    type: String,
    requeired: true,
  },
  descript: {
    type: String,
    requeired: true,
  },
});
const show = defineModel({ required: true, default: false });

const emit = defineEmits<{
  (e: "action"): void;
}>();

const cancel = () => {
  show.value = false;
};

const confirm = () => {
  emit("action");
};
</script>
