import { useAsyncData } from "./useAsyncData";
import { supportCategoryRepository } from "../../infrastructure/repositories/supportCategoryRepository";

export function useSupportCategories(token, query, refreshKey = 0) {
  return useAsyncData(
    () => supportCategoryRepository.list(token, query),
    [token, refreshKey, query?.includeInactive, query?.pageSize]
  );
}

export function useSupportCategoryDetail(token, categoryId) {
  return useAsyncData(
    () => supportCategoryRepository.getById(token, categoryId),
    [token, categoryId]
  );
}

export { supportCategoryRepository };
