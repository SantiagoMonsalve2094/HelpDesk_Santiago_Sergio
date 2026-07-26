import React, { useState } from "react";
import { priorities, statuses } from "../../domain/constants";
import { formatDate, toLabel } from "../../domain/formatters";
import {
  getEffectiveStatus,
  getVisibleTickets,
  isTicketOverdue,
} from "../../domain/ticketRules";
import { useNow } from "../../application/hooks/useNow";
import { useTickets } from "../../application/hooks/useTickets";
import { Badge, EmptyState } from "../components/Feedback";
import { SupervisorAlerts } from "./SlaPage";
import { CreateTicketForm } from "../features/tickets/CreateTicketForm";
import { TicketDetail } from "../features/tickets/TicketDetail";
export function TicketsPage({ token, user, notify }) {
  const [filters, setFilters] = useState({
    status: "",
    priority: "",
    isOverdue: "",
  });
  const [selectedId, setSelectedId] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const now = useNow();
  const tickets = useTickets(token, filters, refreshKey);

  function refresh(message) {
    if (message) notify(message);
    setRefreshKey((value) => value + 1);
  }

  function handleTicketDeleted(message) {
    setSelectedId("");
    refresh(message);
  }

  const items = getVisibleTickets(tickets.data?.items || [], user);
  const selected = selectedId || items[0]?.id || "";
  const canCreateTicket = user.role !== "technician";

  return (
    <section className="grid-page">
      <div className="stack">
        {user.role === "supervisor" && <SupervisorAlerts token={token} />}
        <div className="panel list-panel">
          <div className="section-head">
            <div>
              <p className="eyebrow">{"Operaci\u00f3n"}</p>
              <h2>Tickets</h2>
            </div>
            {canCreateTicket && (
              <button
                className="primary-button"
                onClick={() => setShowCreate((value) => !value)}
              >
                Nuevo
              </button>
            )}
          </div>
          <div className="filters">
            <select
              value={filters.status}
              onChange={(event) =>
                setFilters({ ...filters, status: event.target.value })
              }
            >
              <option value="">Estado</option>
              {statuses.map((status) => (
                <option key={status} value={status}>
                  {toLabel(status)}
                </option>
              ))}
            </select>
            <select
              value={filters.priority}
              onChange={(event) =>
                setFilters({ ...filters, priority: event.target.value })
              }
            >
              <option value="">Prioridad</option>
              {priorities.map((priority) => (
                <option key={priority} value={priority}>
                  {toLabel(priority)}
                </option>
              ))}
            </select>
            <select
              value={filters.isOverdue}
              onChange={(event) =>
                setFilters({ ...filters, isOverdue: event.target.value })
              }
            >
              <option value="">SLA</option>
              <option value="true">Vencidos</option>
              <option value="false">En plazo</option>
            </select>
          </div>
          {showCreate && (
            <CreateTicketForm
              token={token}
              onDone={() => {
                setShowCreate(false);
                refresh("Ticket creado.");
              }}
            />
          )}
          {tickets.loading && <p className="muted">Cargando tickets...</p>}
          {tickets.error && <div className="error-box">{tickets.error}</div>}
          <div className="ticket-list">
            {items.map((ticket) => (
              <button
                key={ticket.id}
                className={[
                  "ticket-card",
                  selected === ticket.id ? "selected" : "",
                  isTicketOverdue(ticket, now) ? "overdue-card" : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
                onClick={() => setSelectedId(ticket.id)}
              >
                <span className="ticket-card-main">
                  <span className="ticket-number">{ticket.ticketNumber}</span>
                  <strong className="ticket-title" title={ticket.subject}>
                    {ticket.subject}
                  </strong>
                </span>
                <span className="ticket-card-badges">
                  <Badge
                    type="status"
                    value={getEffectiveStatus(ticket, now)}
                  />
                  <Badge type="priority" value={ticket.priority} />
                </span>
                <small className="ticket-date">
                  Vence {formatDate(ticket.slaDeadlineAtUtc)}
                </small>
              </button>
            ))}
            {!tickets.loading && items.length === 0 && (
              <EmptyState
                title="No hay tickets para mostrar"
                description="Cuando existan solicitudes disponibles para tu rol aparecerán en esta lista."
                action={
                  canCreateTicket ? (
                    <button
                      className="secondary-button"
                      onClick={() => setShowCreate(true)}
                    >
                      Crear ticket
                    </button>
                  ) : null
                }
              />
            )}
          </div>
        </div>
      </div>
      <TicketDetail
        token={token}
        user={user}
        ticketId={selected}
        onChanged={refresh}
        onDeleted={handleTicketDeleted}
      />
    </section>
  );
}
