import React, { useState } from "react";
import { priorities } from "../../../domain/constants";
import { toLabel } from "../../../domain/formatters";
import { hasBlankFields } from "../../../domain/validation";
import { useSupportCategories } from "../../../application/hooks/useCategories";
import { ticketRepository } from "../../../application/hooks/useTickets";
export function CreateTicketForm({ token, onDone }) {
  const [form, setForm] = useState({
    subject: "",
    description: "",
    priority: "",
    supportCategoryId: "",
  });
  const categories = useSupportCategories(token, { pageSize: 100 });
  const [error, setError] = useState("");

  async function submit(event) {
    event.preventDefault();
    setError("");
    if (
      hasBlankFields(
        form.subject,
        form.description,
        form.supportCategoryId,
        form.priority,
      )
    ) {
      setError("Completa el asunto, la descripción, la categoría y la prioridad.");
      return;
    }
    try {
      await ticketRepository.create(token, form);
      onDone();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form className="inline-form" onSubmit={submit} noValidate>
      <input
        placeholder="Asunto"
        value={form.subject}
        onChange={(e) => setForm({ ...form, subject: e.target.value })}
        required
      />
      <textarea
        placeholder="Descripción"
        value={form.description}
        onChange={(e) => setForm({ ...form, description: e.target.value })}
        required
      />
      <select
        value={form.supportCategoryId}
        onChange={(e) =>
          setForm({ ...form, supportCategoryId: e.target.value })
        }
        required
      >
        <option value="">Categoría</option>
        {(categories.data?.items || []).map((category) => (
          <option key={category.id} value={category.id}>
            {category.name}
          </option>
        ))}
      </select>
      <select
        value={form.priority}
        onChange={(e) => setForm({ ...form, priority: e.target.value })}
        required
      >
        <option value="">Prioridad</option>
        {priorities.map((priority) => (
          <option key={priority} value={priority}>
            {toLabel(priority)}
          </option>
        ))}
      </select>
      {error && <div className="error-box">{error}</div>}
      <button className="primary-button">Crear ticket</button>
    </form>
  );
}
