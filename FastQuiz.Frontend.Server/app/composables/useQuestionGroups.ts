import { ref, toValue, watch, type MaybeRefOrGetter } from 'vue';

/** Modal data must never load without a tenant, or survive a tenant/modal change. */
export function useQuestionGroups(tenantId: MaybeRefOrGetter<string>, enabled: MaybeRefOrGetter<boolean>) {
  const { $api } = useNuxtApp();
  const groups = ref<QuestionGroupDto[]>([]);
  const loadingGroups = ref(false);

  watch([() => toValue(tenantId), () => toValue(enabled)], async ([tenant, open], _, onCleanup) => {
    let current = true;
    onCleanup(() => { current = false; });
    groups.value = [];
    loadingGroups.value = false;
    if (!open || !tenant?.trim()) return;
    loadingGroups.value = true;
    try {
      const all: QuestionGroupDto[] = [];
      const seen = new Set<string>();
      let cursor: string | undefined;
      for (let page = 0; page < 1000; page++) {
        const response = await $api.tenant.questionGroupList(tenant, { Cursor: cursor, Count: 200, IsPrevious: false });
        if (!current) return;
        const batch = response.data ?? [];
        all.push(...batch);
        if (batch.length < 200) break;
        const next = batch.at(-1)?.id;
        if (!next || seen.has(next)) throw new Error('题库分页游标未前进');
        seen.add(next);
        cursor = next;
        if (page === 999) throw new Error('题库分页超出安全上限');
      }
      groups.value = all;
    } catch (error) {
      if (current) console.error('加载题库失败:', error);
    } finally {
      if (current) loadingGroups.value = false;
    }
  }, { immediate: true });
  return { groups, loadingGroups };
}
