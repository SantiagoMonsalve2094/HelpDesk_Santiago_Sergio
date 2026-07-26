import React, { useState } from "react";
import { formatDate, formatDuration, toLabel } from "../../../domain/formatters";
import { getCurrentSlaCycle, getEffectiveStatus, getTicketDeadline } from "../../../domain/ticketRules";
import { useNow } from "../../../application/hooks/useNow";
import { useTicketDetail } from "../../../application/hooks/useTickets";
import { Badge, HistoryBlock, Metric } from "../../components/Feedback";
import { SlaCountdown } from "./SlaCountdown";
import { TicketAdminPanel } from "./TicketAdminPanel";
import { TicketActions } from "./TicketActions";
export function TicketDetail({ token, user, ticketId, onChanged, onDeleted }) {
  const [refreshKey, setRefreshKey] = useState(0);
  const detail = useTicketDetail(token, ticketId, refreshKey);
  const now = useNow();

  if (!ticketId)
    return (
      <div className="panel detail-panel">
        <p className="muted">No hay tickets para mostrar.</p>
      </div>
    );
  if (detail.loading)
    return (
      <div className="panel detail-panel">
        <p className="muted">Cargando detalle...</p>
      </div>
    );
  if (detail.error)
    return (
      <div className="panel detail-panel">
        <div className="error-box">{detail.error}</div>
      </div>
    );
  if (!detail.data)
    return (
      <div className="panel detail-panel">
        <p className="muted">Selecciona un ticket para ver el detalle.</p>
      </div>
    );

  const ticket = detail.data;
  const currentSla = getCurrentSlaCycle(ticket);
  const effectiveStatus = getEffectiveStatus(ticket, now);
  const refresh = (message) => {
    setRefreshKey((value) => value + 1);
    onChanged(message);
  };

  return (
    <div className="panel detail-panel">
      <div className="section-head">
        <div>
          <p className="eyebrow">{ticket.ticketNumber}</p>
          <h2>{ticket.subject}</h2>
        </div>
        <Badge type="status" value={effectiveStatus} />
      </div>
      <p className="detail-description">{ticket.description}</p>
      <div className="metric-row">
        <Metric label="Prioridad" value={toLabel(ticket.priority)} />
        <Metric label="Estado" value={toLabel(effectiveStatus)} />
        <Metric
          label="Fecha SLA"
          value={formatDate(getTicketDeadline(ticket))}
        />
        <Metric
          label="Tiempo máximo"
          value={formatDuration(currentSla?.duration)}
        />
      </div>
      <SlaCountdown ticket={ticket} now={now} />
      <TicketAdminPanel
        token={token}
        user={user}
        ticket={ticket}
        onDone={refresh}
        onDeleted={onDeleted}
      />
      <TicketActions
        token={token}
        user={user}
        ticket={ticket}
        onDone={refresh}
      />
      <div className="two-columns">
        <HistoryBlock
          title="Comentarios"
          rows={ticket.comments}
          render={(row) =>
            `${formatDate(row.createdAtUtc)} - ${row.authorName || "Usuario"}: ${row.body}`
          }
        />
        <HistoryBlock
          title="Estados"
          rows={ticket.statusHistory}
          render={(row) =>
            `${formatDate(row.changedAtUtc)} - ${toLabel(row.newStatus)}`
          }
        />
      </div>
    </div>
  );
}
