<template>
  <CommonManagePageContainer>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />
    <div class="flex flex-col gap-5 p-4">
      <QuestionRecordViewCard
        v-for="(item, index) in questions"
        :key="item.id || index"
        :question="item"
        :index="index + 1"
      />
    </div>
  </CommonManagePageContainer>
</template>

<script setup lang="ts">
//TODO 增加展示评分规则的文本方法
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";

const toast = useToast();
const route = useRoute();

const templateId = ref<string>("");

const template = ref<QuizTemplateDto>({});

const quizSnapshotId = ref<string>("");
const tenantId = ref<string>("");

const quizSnapshot = ref<QuizSnapshotDto>({});

const links = ref<BreadcrumbItem[]>([]);

onMounted(async () => {
  templateId.value = route.params.templateId as string;
  quizSnapshotId.value = route.params.snapshotId as string;
  tenantId.value = route.params.tenantId as string;

  await fetchQuizTemplate();
  await fetchQuizSnapshot();

  await fetchList(200, false);

  links.value.push({
    label: "试卷模板",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/quiz-template`,
  });

  links.value.push({
    label: template.value.name as string,
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}`,
  });

  links.value.push({
    label: "试卷样本列表",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}/snapshots`,
  });

  links.value.push({
    label: quizSnapshot.value.id as string,
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}/snapshots/${quizSnapshotId.value}`,
  });
});

const fetchQuizTemplate = async () => {
  var result = await $api.tenant.quizTemplateDetail(
    tenantId.value,
    templateId.value,
  );
  template.value = result.data;
};

const fetchQuizSnapshot = async () => {
  var result = await $api.tenant.quizTemplateQuizSnapshotDetail(
    tenantId.value,
    quizSnapshotId.value,
  );
  quizSnapshot.value = result.data;
};

const questions = ref<QuestionSnapshotDto[]>([]);
const haveMore = ref<boolean>(true);

const fetchList = async (pageSize: number, refresh: boolean = false) => {
  let cursor = getCursor(false);
  if (refresh) {
    cursor = undefined;
    // 如果是刷新，清空现有列表
    questions.value = [];
  }

  let result = await $api.tenant.quizTemplateQuizSnapshotQuestionSnapshotList(
    tenantId.value,
    quizSnapshotId.value,
    {
      Cursor: cursor,
      Count: pageSize,
    },
  );

  // 拼接新数据而不是替换
  if (refresh) {
    // 刷新模式：直接替换
    questions.value = result.data;
  } else {
    questions.value = [...questions.value, ...result.data];
  }

  haveMore.value = result.data.length == pageSize;
};

const getCursor = (isPre: boolean) => {
  if (questions.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return questions.value[0]?.id as string;
  } else {
    return questions.value[questions.value.length - 1]?.id as string;
  }
};
</script>

<style scoped></style>
