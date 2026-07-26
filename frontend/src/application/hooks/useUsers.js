import { useAsyncData } from "./useAsyncData";
import { userRepository } from "../../infrastructure/repositories/userRepository";

export function useUsers(token, query, refreshKey) {
  return useAsyncData(
    () => userRepository.list(token, query),
    [token, refreshKey, query?.pageSize, query?.role, query?.supportCategoryId, query?.isActive]
  );
}

export function useUserDetail(token, userId, enabled, refreshKey) {
  return useAsyncData(
    () => enabled ? userRepository.getById(token, userId) : Promise.resolve(null),
    [token, userId, enabled, refreshKey]
  );
}

export { userRepository };
