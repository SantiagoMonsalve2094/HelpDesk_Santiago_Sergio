import { useAsyncData } from "./useAsyncData";
import { slaRepository } from "../../infrastructure/repositories/slaRepository";

export function useSlaReport(token, filters = {}) {
  return useAsyncData(
    () => slaRepository.report(token, filters),
    [token, filters.supportCategoryId, filters.technicianUserId, filters.fromUtc, filters.toUtc]
  );
}

export function useSlaAlerts(token, query = {}) {
  return useAsyncData(
    () => slaRepository.alerts(token, query),
    [token, query.pageSize]
  );
}
