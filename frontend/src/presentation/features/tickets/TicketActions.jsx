import React, { useState } from "react";
import { canAssign, canClose, canReassign, canReopen, canResolve, canStart, hasAvailableCapacity, isPrivileged, technicianSupportsTicket } from "../../../domain/ticketRules";
import { hasBlankFields } from "../../../domain/validation";
import { useNow } from "../../../application/hooks/useNow";
import { useAssignableTechnicians, ticketRepository } from "../../../application/hooks/useTickets";
export function TicketActions({ token, user, ticket, onDone }) {
  const [comment, setComment] = useState("");
  const [resolution, setResolution] = useState("");
  const [closeComment, setCloseComment] = useState("");
  const [assignTo, setAssignTo] = useState("");
  const [reason, setReason] = useState("");
  const [error, setError] = useState("");
  const now = useNow();
  const technicians = useAssignableTechnicians(
    token,
    ticket.id,
    isPrivileged(user.role),
  );
  const availableTechnicians = (technicians.data || []).filter((technician) =>
    technicianSupportsTicket(technician, ticket),
  );

  async function post(path, body, message) {
    setError("");
    if (body && hasBlankFields(...Object.values(body))) {
      setError("Completa los campos obligatorios.");
      return false;
    }
    try {
      await path();
      setComment("");
      setResolution("");
      setCloseComment("");
      setReason("");
      onDone(message);
      return true;
    } catch (err) {
      setError(err.message);
      return false;
    }
  }

  async function closeTicket(event) {
    event.preventDefault();
    const commentSaved = await post(
      () => ticketRepository.addComment(token, ticket.id, closeComment),
      { text: closeComment },
      "Comentario de cierre registrado.",
    );
    if (commentSaved) {
      await post(
        () => ticketRepository.close(token, ticket.id),
        null,
        "Ticket cerrado.",
      );
    }
  }

  const shouldShowAssignment =
    canAssign(ticket, user) || canReassign(ticket, user);
  const isReassignment = canReassign(ticket, user);

  return (
    <div className="actions">
      {error && <div className="error-box">{error}</div>}
      <div className="action-grid">
        <form
          noValidate
          onSubmit={(e) => {
            e.preventDefault();
            post(
              () => ticketRepository.addComment(token, ticket.id, comment),
              { text: comment },
              "Comentario registrado.",
            );
          }}
        >
          <input
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder="Comentario"
            required
          />
          <button className="secondary-button">Comentar</button>
        </form>
        {canResolve(ticket, user) && (
          <form
            noValidate
            onSubmit={(e) => {
              e.preventDefault();
              post(
                () => ticketRepository.resolve(token, ticket.id, resolution),
                { resolutionComment: resolution },
                "Ticket resuelto.",
              );
            }}
          >
            <input
              value={resolution}
              onChange={(e) => setResolution(e.target.value)}
              placeholder="Comentario de resolución"
              required
            />
            <button className="secondary-button">Resolver</button>
          </form>
        )}
        {shouldShowAssignment && (
          <form
            noValidate
            onSubmit={(e) => {
              e.preventDefault();
              post(
                () =>
                  isReassignment
                    ? ticketRepository.reassign(
                        token,
                        ticket.id,
                        assignTo,
                        reason,
                      )
                    : ticketRepository.assign(token, ticket.id, assignTo),
                isReassignment
                  ? { technicianUserId: assignTo, reason }
                  : { technicianUserId: assignTo },
                "Asignación actualizada.",
              );
            }}
          >
            <select
              value={assignTo}
              onChange={(e) => setAssignTo(e.target.value)}
              required
            >
              <option value="">Técnico</option>
              {availableTechnicians.map((tech) => (
                <option
                  key={tech.technicianUserId}
                  value={tech.technicianUserId}
                  disabled={!hasAvailableCapacity(tech)}
                >
                  {tech.fullName} ({tech.activeTicketCount}/
                  {tech.maxActiveTickets}){" "}
                  {!hasAvailableCapacity(tech) ? " - Capacidad Máxima" : ""}
                </option>
              ))}
            </select>
            {isReassignment && (
              <input
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                placeholder="Motivo"
                required
              />
            )}
            <button className="secondary-button">
              {isReassignment ? "Reasignar" : "Asignar"}
            </button>
          </form>
        )}
      </div>
      <div className="button-row">
        {canStart(ticket, user) && (
          <button
            className="ghost-button"
            onClick={() =>
              post(
                () => ticketRepository.start(token, ticket.id),
                null,
                "Ticket en proceso.",
              )
            }
          >
            Iniciar
          </button>
        )}
        {canReopen(ticket, user, now) && (
          <button
            className="ghost-button"
            onClick={() =>
              post(
                () => ticketRepository.reopen(token, ticket.id),
                null,
                "Ticket reabierto.",
              )
            }
          >
            Reabrir
          </button>
        )}
      </div>
      {canClose(ticket, user) && (
        <form className="close-form" noValidate onSubmit={closeTicket}>
          <textarea
            value={closeComment}
            onChange={(e) => setCloseComment(e.target.value)}
            placeholder="Comentario de cierre"
          />
          <button
            className="primary-button"
            disabled={closeComment.trim().length === 0}
          >
            Cerrar Ticket
          </button>
        </form>
      )}
    </div>
  );
}
