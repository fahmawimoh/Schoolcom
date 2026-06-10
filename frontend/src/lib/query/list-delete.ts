import type { QueryClient } from "@tanstack/react-query";

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** Immediately remove a deleted row from cached paginated list queries. */
export function applyDeleteToListCache<T extends { id: string }>(
  queryClient: QueryClient,
  listQueryKeyPrefix: readonly unknown[],
  deletedId: string
): void {
  queryClient.setQueriesData<PaginatedList<T>>(
    { queryKey: listQueryKeyPrefix },
    (current) => {
      if (!current?.items.some((item) => item.id === deletedId)) {
        return current;
      }

      const items = current.items.filter((item) => item.id !== deletedId);
      const totalCount = Math.max(0, current.totalCount - 1);
      const totalPages = Math.max(
        1,
        Math.ceil(totalCount / current.pageSize)
      );

      return {
        ...current,
        items,
        totalCount,
        totalPages,
      };
    }
  );
}

/** Re-fetch list queries from the API after a mutation. */
export async function refreshListQueries(
  queryClient: QueryClient,
  listQueryKeyPrefix: readonly unknown[],
  extraQueryKeyPrefixes: readonly (readonly unknown[])[] = []
): Promise<void> {
  await queryClient.invalidateQueries({ queryKey: listQueryKeyPrefix });
  await queryClient.refetchQueries({
    queryKey: listQueryKeyPrefix,
    type: "active",
  });

  for (const key of extraQueryKeyPrefixes) {
    await queryClient.invalidateQueries({ queryKey: key });
  }
}
