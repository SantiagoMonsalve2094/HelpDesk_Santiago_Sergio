import { REOPEN_WINDOW_HOURS } from "./constants";

export function isPrivileged(role) {
  return role === "supervisor" || role === "superAdmin";
}

export function getTicketDeadline(ticket) {
  if (ticket?.slaDeadlineAtUtc) return ticket.slaDeadlineAtUtc;
  const pendingCycle = ticket?.slaCycles?.find((cycle) => cycle.outcome === "pending");
  return pendingCycle?.deadlineAtUtc || ticket?.slaCycles?.[0]?.deadlineAtUtc;
}

export function getCurrentSlaCycle(ticket) {
  return ticket?.slaCycles?.find((cycle) => cycle.outcome === "pending") || ticket?.slaCycles?.[0] || null;
}

export function isTicketOverdue(ticket, now = new Date()) {
  if (!ticket || ticket.status === "resolved" || ticket.status === "closed") return false;
  const deadline = getTicketDeadline(ticket);
  return Boolean(ticket.isOverdue || (deadline && new Date(deadline) < now));
}

export function getEffectiveStatus(ticket, now = new Date()) {
  return isTicketOverdue(ticket, now) ? "overdue" : ticket?.status;
}

export function getVisibleTickets(tickets, user) {
  if (!user) return [];
  if (user.role === "user") return tickets.filter((ticket) => ticket.creatorUserId === user.id);
  if (user.role === "technician") {
    return tickets.filter((ticket) => ticket.currentTechnicianUserId === user.id);
  }
  return tickets;
}

export function canAssign(ticket, user) {
  return isPrivileged(user.role) && ["open", "reopened"].includes(ticket.status);
}

export function canReassign(ticket, user) {
  return isPrivileged(user.role) && ["assigned", "inProgress", "reopened"].includes(ticket.status) && Boolean(ticket.currentTechnicianUserId);
}

export function canStart(ticket, user) {
  return user.role === "technician" &&
    ticket.currentTechnicianUserId === user.id &&
    ["assigned", "reopened"].includes(ticket.status);
}

export function canResolve(ticket, user) {
  return user.role === "technician" &&
    ticket.currentTechnicianUserId === user.id &&
    ["inProgress", "reopened"].includes(ticket.status);
}

export function canClose(ticket, user) {
  return ticket.status === "resolved" && ticket.creatorUserId === user.id && hasResolutionEvidence(ticket);
}

export function hasResolutionEvidence(ticket) {
  return Boolean(ticket?.comments?.some((comment) =>
    comment.satisfiesResolutionRequirement &&
    comment.authorUserId === ticket.currentTechnicianUserId
  ));
}

export function canReopen(ticket, user, now = new Date()) {
  if (user.role !== "user" || ticket.creatorUserId !== user.id || ticket.status !== "resolved" || !ticket.resolvedAtUtc) {
    return false;
  }
  const reopenUntil = new Date(ticket.resolvedAtUtc);
  reopenUntil.setHours(reopenUntil.getHours() + REOPEN_WINDOW_HOURS);
  return now <= reopenUntil;
}

export function technicianSupportsTicket(technician, ticket) {
  const specialties = technician.supportCategoryIds || technician.specialties || technician.specialityIds;
  if (!Array.isArray(specialties) || specialties.length === 0) return true;
  return specialties.some((value) => String(value).toLowerCase() === String(ticket.supportCategoryId).toLowerCase());
}

export function hasAvailableCapacity(technician) {
  if (typeof technician.availableCapacity === "number") return technician.availableCapacity > 0;
  if (typeof technician.activeTicketCount === "number" && typeof technician.maxActiveTickets === "number") {
    return technician.activeTicketCount < technician.maxActiveTickets;
  }
  return true;
}
