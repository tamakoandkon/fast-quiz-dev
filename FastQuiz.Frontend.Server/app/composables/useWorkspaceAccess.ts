export const useWorkspaceAccess = () => {
  const route = useRoute();
  const { $api } = useNuxtApp();
  const permissions = useState<string[]>('workspace-permissions', () => []);
  const permissionsTenant = useState<string>('workspace-permissions-tenant', () => '');
  const token = useCookie<string | null>('cw_auth_token');
  const tenantId = computed(() => String(route.params.tenantId || ''));
  const can = (permission?: string) => !permission || (
    permissionsTenant.value === tenantId.value && permissions.value.includes(permission)
  );
  const loadPermissions = async () => {
    const id = tenantId.value;
    const requestedToken = token.value;
    permissions.value = [];
    permissionsTenant.value = '';
    if (!id) return;
    try {
      const response = await $api.tenant.rolePermissionMeList(id);
      if (tenantId.value === id && token.value === requestedToken) {
        permissions.value = response.data;
        permissionsTenant.value = id;
      }
    } catch {
      // Basic exam navigation remains available if administrative access is unavailable.
    }
  };
  return { tenantId, permissions, can, loadPermissions };
};
