import React from "react";
import { formatDate } from "../../../domain/formatters";
import { getTicketDeadline, isTicketOverdue } from "../../../domain/ticketRules";

export function SlaCountdown({ ticket, now }) {
  const deadline = getTicketDeadline(ticket);
  if (!deadline) return null;
  const remainingMs = new Date(deadline).getTime() - now.getTime();
  const isOverdue = isTicketOverdue(ticket, now);
  const absMinutes = Math.max(0, Math.floor(Math.abs(remainingMs) / 60000));
  const hours = Math.floor(absMinutes / 60);
  const minutes = absMinutes % 60;
  const text = isOverdue
    ? `SLA superado hace ${hours} h ${minutes} min`
    : `Tiempo restante: ${hours} h ${minutes} min`;

  return (
    <div className={isOverdue ? "sla-banner danger" : "sla-banner"}>
      <strong>{isOverdue ? "Alerta de vencimiento" : "SLA en curso"}</strong>
      <span>{text}</span>
      <small>Fecha límite: {formatDate(deadline)}</small>
    </div>
  );
}
