<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" class="py-2" />
    </template>

    <CommonButtonsContainer>
      <QuizSnapshotCreateQuizSnapshotModal
        v-model="showCreateQuizSnapshot"
        @create="createQuizSnapshot"
        :quiz-template-id="templateId"
      />
    </CommonButtonsContainer>

    <QuizAttemptCreateQuizAttemptModal
      class="mx-2"
      :quiz-template-id="templateId"
      :quiz-snapshot-id="selectedId"
      :busy="generating"
      @create="handleCreateAttempt"
      v-model="showCreateCreateQuizAttempt"
    ></QuizAttemptCreateQuizAttemptModal>
    <CommonDataTable
      :columns="cols"
      :data="snapshots"
      :selectable="true"
      v-model="selected"
      @row-dbl-click="
        (row) =>
          router.push(
            `/tenant/${tenantId}/dash/quiz-template/${templateId}/snapshots/${row.id}`,
          )
      "
    >
      <template #id="{ row }">
        <CommonTruncateText :text="row.id || ''" direction="rtl">
          <NuxtLink
            :to="`/tenant/${tenantId}/dash/quiz-template/${templateId}/snapshots/${row.id}`"
            class="text-primary-600 hover:text-primary-700 hover:underline cursor-pointer"
          >
            {{ row.id }}
          </NuxtLink>
        </CommonTruncateText>
      </template>
      <template #actions="{ row }">
        <div class="flex justify-center gap-3">
          <!-- <button
            @click="handleEdit(row)"
            class="text-blue-500 hover:text-blue-700 font-medium"
          >
            编辑
          </button>
          <button
            @click="deleteQuizTemplate(row.id ?? '')"
            class="text-red-500 hover:text-red-700 font-medium"
          >
            删除
          </button> -->
          <UButton :disabled="generating" @click="() => createAttempt(row.id ?? '')">批量生成</UButton>
        </div>
      </template>
    </CommonDataTable>
    <CommonLoadMoreButton
      v-if="haveMore"
      @click="
        () => {
          fetchList(pageSize);
        }
      "
    ></CommonLoadMoreButton>
    <section class="workspace-card mt-6 space-y-4 p-5">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h2 class="text-base font-semibold">待领取试卷</h2>
          <p class="text-sm text-muted">已加载 {{ pool.length }} 份。发布考试后，符合资格的考生可在考试中心领取。</p>
        </div>
        <UButton color="neutral" variant="soft" :loading="poolBusy" @click="fetchPool(true)">刷新</UButton>
      </div>
      <p v-if="poolError" role="alert" class="text-sm text-error">{{ poolError }}</p>
      <CommonDataTable :columns="poolColumns" :data="pool" />
      <p v-if="!poolBusy && !poolError && pool.length === 0" class="text-sm text-muted">暂无待领取试卷，请从上方样本批量生成。</p>
      <UButton v-if="poolHasMore" color="neutral" variant="ghost" :loading="poolBusy" @click="fetchPool(false)">加载更多待领取试卷</UButton>
    </section>
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";

const toast = useToast();
const route = useRoute();
const router = useRouter();

const templateId = ref<string>("");
const tenantId = ref<string>("");

const template = ref<QuizTemplateDto>({});

const links = ref<BreadcrumbItem[]>([]);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  templateId.value = route.params.templateId as string;

  await fetchQuizTemplate();

  await fetchList(pageSize.value, true);
  await fetchPool(true);

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
});

const fetchQuizTemplate = async () => {
  var result = await $api.tenant.quizTemplateDetail(
    tenantId.value,
    templateId.value,
  );
  template.value = result.data;
};

const snapshots = ref<QuizSnapshotDto[]>([]);
const selected = ref<QuizSnapshotDto[]>([]);

const pageSize = ref(50);

const haveMore = ref(true);

const cols = [
  { key: "id", label: "ID", width: "16px" },
  { key: "totalScore", label: "总分数", width: "16px" },
  { key: "actions", label: "操作", width: "150px" },
];

const fetchList = async (pageSize: number, refresh: boolean = false) => {
  if (!haveMore.value) {
    return;
  }

  let cursor = getCursor();
  if (refresh) {
    cursor = undefined;
    snapshots.value = [];
  }

  let result = await $api.tenant.quizTemplateQuizSnapshotList(
    tenantId.value,
    templateId.value,
    {
      Cursor: cursor,
      Count: pageSize,
    },
  );

  if (result.data.length !== pageSize) {
    haveMore.value = false;
    if (!refresh) {
      toast.add({ title: "没有更多数据了", color: "warning" });
    }
  }

  if (refresh) {
    // 刷新时直接替换
    snapshots.value = result.data;
  } else {
    // 非刷新时追加到列表后面
    snapshots.value = [...snapshots.value, ...result.data];
  }
};

const getCursor = () => {
  if (snapshots.value.length == 0) {
    return undefined;
  }
  // 使用最后一条记录的ID作为游标（向后加载）
  return snapshots.value[snapshots.value.length - 1]?.id as string;
};

const showCreateCreateQuizAttempt = ref(false);
const selectedId = ref("");
const generating = ref(false);
const pool = ref<PrebuiltQuizAttemptDto[]>([]);
const poolBusy = ref(false);
const poolError = ref("");
const poolHasMore = ref(false);
const poolColumns = [{ key: "id", label: "试卷 ID" }, { key: "quizSnapshotId", label: "来源样本" }];
const fetchPool = async (refresh: boolean) => {
  if (poolBusy.value) return;
  poolBusy.value = true;
  poolError.value = "";
  try {
    const result = await $api.tenant.quizTemplateQuizAttemptPrebuiltList(tenantId.value, templateId.value, {
      Cursor: refresh ? undefined : (pool.value.at(-1)?.id ?? undefined),
      Count: 50,
    });
    pool.value = refresh ? result.data : [...pool.value, ...result.data];
    poolHasMore.value = result.data.length === 50;
  } catch (e: any) {
    poolError.value = e?.response?.data?.title ?? "待领取试卷加载失败";
  } finally { poolBusy.value = false; }
};

const handleCreateAttempt = async (
  quizTemplateId: string,
  command: CreateQuizAttemptCommand,
) => {
  if (generating.value) return;
  generating.value = true;
  try {
    await $api.tenant.quizTemplateQuizAttemptBatchCreate(tenantId.value, quizTemplateId, [command]);
    showCreateCreateQuizAttempt.value = false;
    toast.add({ title: `已生成 ${command.count} 份待领取试卷`, color: "success" });
    await fetchPool(true);
  } catch (e: any) {
    toast.add({ title: e?.response?.data?.title ?? "试卷生成失败", color: "error" });
  } finally { generating.value = false; }
};

const createAttempt = (snapshotId: string) => {
  selectedId.value = snapshotId;
  showCreateCreateQuizAttempt.value = true;
};

const showCreateQuizSnapshot = ref(false);

const createQuizSnapshot = async (
  quizTemplateId: string,
  command: CreateQuizSnapshotCommand,
) => {
  try {
    await $api.tenant.quizTemplateQuizSnapshotCreate(
      tenantId.value,
      quizTemplateId,
      command,
    );
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "试卷样本创建失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "试卷样本创建成功" });

  showCreateQuizSnapshot.value = false;
  haveMore.value = true;
  await fetchList(pageSize.value, true);
};
</script>

<style scoped></style>
