<template>
  <CommonPageLayout>
    <template #actions>
      <div class="page-search">
        <CommonSearchInput v-model="searchInput" @search="handleSearch" />
      </div>
      <QuestionCreateQuestionGroupModal
        v-model="showCreateModal"
        @create="createGroup"
      />
    </template>

    <QuestionUpdateQuestionGroupModal
      v-model="showUpdateModal"
      @update="updateGroup"
      :id="currentId"
      :tenant-id="tenantId"
    />

    <CommonConfirmModal
      v-model="showConfiemModal"
      @action="handleDelete"
      title="是否删除题库"
      descript="题库删除后，其中的题目也会一并移除。"
    />

    <CommonConfirmModal
      v-model="showConfirmBatchDeleteGroupsModal"
      @action="handleBatchDeleteGroup"
      title="是否删除所选题库"
      :descript="`将删除 ${selected.length} 个题库，删除后无法恢复。`"
    />

    <!-- 选中后浮出的操作条，替代常驻的禁用按钮 -->
    <div v-if="selected.length" class="selection-bar" role="status">
      <p class="selection-bar-text">已选择 {{ selected.length }} 个题库</p>
      <div class="selection-bar-actions">
        <UButton color="neutral" variant="ghost" size="sm" @click="clearSelection">取消选择</UButton>
        <UButton color="error" variant="soft" size="sm" icon="i-lucide-trash-2" @click="batchDeleteGroups">删除所选</UButton>
      </div>
    </div>

    <section
      class="data-list is-bank"
      :class="{ 'is-busy': refreshing }"
      :aria-busy="refreshing"
      aria-label="题库列表"
    >
      <div v-if="loading" class="data-list-skeleton" role="status" aria-label="正在加载题库">
        <div v-for="n in 4" :key="n" class="data-list-skeleton-row">
          <div class="data-list-main data-list-skeleton-main">
            <span class="skeleton-line"></span>
            <span class="skeleton-line is-short is-small"></span>
          </div>
          <span class="skeleton-line is-short"></span>
          <span class="skeleton-line is-short"></span>
          <span class="skeleton-line is-short"></span>
        </div>
      </div>

      <div v-else-if="loadError && !groups.length" class="data-list-state">
        <UIcon name="i-lucide-cloud-off" aria-hidden="true" />
        <p>题库列表暂时无法加载。</p>
        <p class="data-list-state-hint">检查网络连接后重试。</p>
        <UButton color="neutral" variant="outline" size="sm" icon="i-lucide-refresh-cw" @click="fetchList(false, pageSize, true)">
          重新加载
        </UButton>
      </div>

      <div v-else-if="!groups.length" class="data-list-state">
        <UIcon name="i-lucide-library" aria-hidden="true" />
        <p>{{ searchInput ? '没有匹配的题库。' : '还没有题库。' }}</p>
        <p class="data-list-state-hint">
          {{ searchInput ? '换个关键词，或清空搜索查看全部。' : '创建第一个题库后，就能往里面录入题目。' }}
        </p>
      </div>

      <template v-else>
        <div class="data-list-table">
          <div class="data-list-head" role="row">
            <span class="data-list-main" role="columnheader">
              <input
                class="data-list-check"
                type="checkbox"
                :checked="isAllSelected"
                :indeterminate="isIndeterminate"
                aria-label="选择本页全部题库"
                @change="toggleAll"
              />
              <span>题库</span>
            </span>
            <span role="columnheader">题目数</span>
            <span role="columnheader">创建者</span>
            <span role="columnheader">操作</span>
          </div>

          <div class="data-list-body" role="rowgroup">
            <article
              v-for="group in groups"
              :key="group.id || ''"
              class="data-list-row"
              :class="{ 'is-selected': isSelected(group) }"
              role="row"
            >
              <div class="data-list-main" role="cell">
                <input
                  class="data-list-check"
                  type="checkbox"
                  :checked="isSelected(group)"
                  :aria-label="`选择 ${group.name || '未命名题库'}`"
                  @change="toggleRow(group)"
                />
                <div class="data-list-text">
                  <NuxtLink
                    :to="`/tenant/${tenantId}/dash/question-group/${group.id}`"
                    class="data-list-title"
                    :title="group.name || '未命名题库'"
                  >
                    {{ group.name || "未命名题库" }}
                  </NuxtLink>
                  <p v-if="group.remark" class="data-list-note" :title="group.remark">
                    {{ group.remark }}
                  </p>
                </div>
              </div>

              <div class="data-list-status-cell" role="cell">
                <span class="data-list-count">
                  {{ group.count ?? 0 }}<small>题</small>
                </span>
              </div>

              <span
                class="data-list-aside"
                role="cell"
                :title="group.ownerId || '未记录创建者'"
              >
                <span class="data-list-aside-label">创建者</span>
                <span class="data-list-aside-value">{{ group.ownerId || "—" }}</span>
              </span>

              <div class="data-list-actions" role="cell">
                <UButton
                  class="data-list-edit"
                  color="neutral"
                  variant="ghost"
                  size="sm"
                  icon="i-lucide-pen-line"
                  @click="openEdit(group)"
                >
                  编辑
                </UButton>
                <UDropdownMenu :items="rowMenu(group)" :content="{ align: 'end' }">
                  <UButton
                    class="data-list-kebab"
                    color="neutral"
                    variant="ghost"
                    size="sm"
                    icon="i-lucide-ellipsis"
                    aria-label="更多操作"
                  />
                </UDropdownMenu>
              </div>
            </article>
          </div>
        </div>

        <CommonListFooter
          :page-index="pageIndex"
          :page-size="pageSize"
          :item-count="groups.length"
          unit="个"
          :can-prev="canPrev"
          :can-next="canNext"
          :busy="refreshing"
          @prev="goPrev"
          @next="goNext"
          @update:page-size="onPageSize"
        />
      </template>
    </section>
  </CommonPageLayout>
</template>

<script setup lang="ts">
import type { DropdownMenuItem } from "@nuxt/ui";

const { $api } = useNuxtApp();
const toast = useToast();
const route = useRoute();
const router = useRouter();

const tenantId = ref("");
const groups = ref<QuestionGroupDto[]>([]);
const pageSize = ref(10);
const pageIndex = ref(0);
const searchInput = ref("");
const selected = ref<QuestionGroupDto[]>([]);

const loading = ref(true);
const refreshing = ref(false);
const loadError = ref(false);

const currentId = ref("");
const showConfiemModal = ref(false);
const showConfirmBatchDeleteGroupsModal = ref(false);
const showCreateModal = ref(false);
const showUpdateModal = ref(false);

const canPrev = computed(() => pageIndex.value > 0 && !refreshing.value);
const canNext = computed(
  () => groups.value.length === pageSize.value && !refreshing.value,
);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  await fetchList(false, pageSize.value, true);
});

const getCursor = (isPre: boolean) => {
  if (groups.value.length == 0) return undefined;
  return isPre
    ? (groups.value[0]?.id as string)
    : (groups.value[groups.value.length - 1]?.id as string);
};

const fetchList = async (
  isPre: boolean,
  size: number,
  refresh: boolean = false,
  namePrefix: string | undefined = undefined,
) => {
  if (refresh) pageIndex.value = 0;
  const cursor = refresh ? undefined : getCursor(isPre);

  if (!groups.value.length) loading.value = true;
  else refreshing.value = true;
  loadError.value = false;

  try {
    const result = await $api.tenant.questionGroupList(tenantId.value, {
      Cursor: cursor,
      Count: size,
      IsPrevious: isPre,
      NamePrefix: namePrefix,
    });
    const batch = result.data ?? [];

    // 游标分页没有总数；反向空页说明已经到头，保留现有行
    if (!refresh && batch.length === 0) {
      toast.add({ title: "没有更多题库了", color: "neutral" });
      return false;
    }

    groups.value = batch;
    selected.value = [];
    return true;
  } catch (e: any) {
    loadError.value = true;
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `获取题库失败：${title}`, color: "error" });
    return false;
  } finally {
    loading.value = false;
    refreshing.value = false;
  }
};

const goPrev = async () => {
  if (!canPrev.value) return;
  if (await fetchList(true, pageSize.value, false)) {
    pageIndex.value = Math.max(0, pageIndex.value - 1);
  }
};

const goNext = async () => {
  if (!canNext.value) return;
  if (await fetchList(false, pageSize.value, false)) {
    pageIndex.value += 1;
  }
};

const onPageSize = async (next: number) => {
  pageSize.value = next;
  pageIndex.value = 0;
  await fetchList(false, next, true);
};

const handleSearch = async (value: string | undefined) => {
  await fetchList(false, pageSize.value, true, value);
};

// ---------------- 选择 ----------------

const isSelected = (row: QuestionGroupDto) =>
  selected.value.some((item) => item.id === row.id);

const toggleRow = (row: QuestionGroupDto) => {
  if (selected.value.some((item) => item.id === row.id)) {
    selected.value = selected.value.filter((item) => item.id !== row.id);
    return;
  }
  selected.value = [...selected.value, row];
};

const isAllSelected = computed(
  () =>
    groups.value.length > 0 &&
    selected.value.length === groups.value.length,
);
const isIndeterminate = computed(
  () => selected.value.length > 0 && selected.value.length < groups.value.length,
);

const toggleAll = () => {
  selected.value = isAllSelected.value ? [] : [...groups.value];
};

const clearSelection = () => {
  selected.value = [];
};

// ---------------- 增删改 ----------------

const createGroup = async (command: CreateQuestionGroupCommand) => {
  try {
    await $api.tenant.questionGroupCreate(tenantId.value, {
      name: command.name,
      remark: command.remark,
    });
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `创建题库失败：${title}`, color: "error" });
    return;
  }

  toast.add({ title: "题库创建成功。" });
  showCreateModal.value = false;
  await fetchList(false, pageSize.value, true, searchInput.value);
};

const openEdit = (row: QuestionGroupDto) => {
  currentId.value = row.id || "";
  showUpdateModal.value = true;
};

const updateGroup = async (command: UpdateQuestionGroupCommand) => {
  try {
    await $api.tenant.questionGroupUpdate(tenantId.value, currentId.value, {
      name: command.name,
      remark: command.remark,
    });
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `修改题库失败：${title}`, color: "error" });
    return;
  }

  toast.add({ title: "题库修改成功。" });
  showUpdateModal.value = false;
  await fetchList(false, pageSize.value, true, searchInput.value);
};

const confirmDelete = (row: QuestionGroupDto) => {
  currentId.value = row.id || "";
  showConfiemModal.value = true;
};

const handleDelete = async () => {
  if (!currentId.value || currentId.value.trim().length == 0) {
    toast.add({ title: "ID 参数异常", color: "error" });
    return;
  }
  try {
    await $api.tenant.questionGroupDelete(tenantId.value, currentId.value);
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `删除题库失败：${title}`, color: "error" });
    return;
  }

  toast.add({ title: "题库删除成功。" });
  showConfiemModal.value = false;
  await fetchList(false, pageSize.value, true, searchInput.value);
};

const batchDeleteGroups = () => {
  if (!selected.value.length) return;
  showConfirmBatchDeleteGroupsModal.value = true;
};

const handleBatchDeleteGroup = async () => {
  const groupIds = selected.value
    .map((item) => item.id)
    .filter((id): id is string => typeof id === "string" && id.trim() !== "");

  if (groupIds.length === 0) return;

  try {
    await $api.tenant.questionGroupBatchDeleteCreate(tenantId.value, {
      groupIds,
    });
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `批量删除题库失败：${title}`, color: "error" });
    return;
  }

  selected.value = [];
  showConfirmBatchDeleteGroupsModal.value = false;
  toast.add({ title: `已删除 ${groupIds.length} 个题库` });
  await fetchList(false, pageSize.value, true, searchInput.value);
};

// ---------------- 行操作 ----------------

const copyId = async (row: QuestionGroupDto) => {
  if (!row.id) return;
  try {
    await navigator.clipboard.writeText(row.id);
    toast.add({ title: "题库 ID 已复制" });
  } catch {
    toast.add({ title: "复制失败，请到详情页手动复制", color: "error" });
  }
};

const rowMenu = (row: QuestionGroupDto): DropdownMenuItem[][] => [
  [
    {
      label: "进入题库",
      icon: "i-lucide-arrow-up-right",
      onSelect: () =>
        router.push(`/tenant/${tenantId.value}/dash/question-group/${row.id}`),
    },
    {
      label: "编辑",
      icon: "i-lucide-pen-line",
      onSelect: () => openEdit(row),
    },
    {
      label: "复制 ID",
      icon: "i-lucide-copy",
      onSelect: () => copyId(row),
    },
  ],
  [
    {
      label: "删除",
      color: "error",
      icon: "i-lucide-trash-2",
      onSelect: () => confirmDelete(row),
    },
  ],
];
</script>
