import { useAsyncData } from "./useAsyncData";
import { ticketRepository } from "../../infrastructure/repositories/ticketRepository";

export function useTickets(token, filters, refreshKey) {
  return useAsyncData(
    () => ticketRepository.list(token, { ...filters, pageSize: 50 }),
    [token, refreshKey, filters.status, filters.priority, filters.isOverdue]
  );
}

export function useTicketDetail(token, ticketId, refreshKey) {
  return useAsyncData(
    () => ticketId ? ticketRepository.getById(token, ticketId) : Promise.resolve(null),
    [token, ticketId, refreshKey]
  );
}

export function useAssignableTechnicians(token, ticketId, enabled) {
  return useAsyncData(
    () => enabled ? ticketRepository.assignableTechnicians(token, ticketId) : Promise.resolve([]),
    [token, ticketId, enabled]
  );
}

export { ticketRepository };
