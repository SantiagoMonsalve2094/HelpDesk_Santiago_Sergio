import React, { useEffect, useState } from "react";
import { hasBlankFields } from "../../../domain/validation";
import { ticketRepository } from "../../../application/hooks/useTickets";
export function TicketAdminPanel({ token, user, ticket, onDone, onDeleted }) {
  const [edit, setEdit] = useState({
    subject: ticket.subject,
    description: ticket.description,
  });
  const [error, setError] = useState("");
  const canEdit =
    user.role === "superAdmin" || ticket.creatorUserId === user.id;
  const canDelete =
    user.role === "superAdmin" ||
    (ticket.creatorUserId === user.id && ticket.status === "open");

  useEffect(() => {
    setEdit({ subject: ticket.subject, description: ticket.description });
    setError("");
  }, [ticket.id, ticket.subject, ticket.description]);

  if (!canEdit && !canDelete) return null;

  async function updateTicket(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(edit.subject, edit.description)) {
      setError("Completa el asunto y la descripción.");
      return;
    }
    try {
      await ticketRepository.update(token, ticket.id, edit);
      onDone("Ticket actualizado.");
    } catch (err) {
      setError(err.message);
    }
  }

  async function deleteTicket() {
    setError("");
    if (!window.confirm("¿Seguro que deseas eliminar este ticket?")) return;
    try {
      await ticketRepository.delete(token, ticket.id);
      onDeleted("Ticket eliminado.");
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div className="admin-panel">
      <div className="section-head compact">
        <div>
          <p className="eyebrow">Administración</p>
          <h3>Edición del ticket</h3>
        </div>
      </div>
      {error && <div className="error-box">{error}</div>}
      {canEdit && (
        <form className="inline-form" onSubmit={updateTicket} noValidate>
          <input
            value={edit.subject}
            onChange={(e) => setEdit({ ...edit, subject: e.target.value })}
            placeholder="Asunto"
            required
          />
          <textarea
            value={edit.description}
            onChange={(e) => setEdit({ ...edit, description: e.target.value })}
            placeholder="Descripción"
            required
          />
          <button className="secondary-button">Guardar ticket</button>
        </form>
      )}
      <div className="button-row">
        {canDelete && (
          <button className="ghost-button danger-button" onClick={deleteTicket}>
            Eliminar ticket
          </button>
        )}
      </div>
    </div>
  );
}
