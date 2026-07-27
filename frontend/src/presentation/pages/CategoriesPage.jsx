import React, { useEffect, useState } from "react";
import { priorities } from "../../domain/constants";
import { durationToMinutes, toLabel } from "../../domain/formatters";
import { isPrivileged } from "../../domain/ticketRules";
import { hasBlankFields } from "../../domain/validation";
import { useSupportCategories, useSupportCategoryDetail, supportCategoryRepository } from "../../application/hooks/useCategories";
export function CategoriesPage({ token, user, notify }) {
  const [refreshKey, setRefreshKey] = useState(0);
  const categories = useSupportCategories(
    token,
    { includeInactive: true, pageSize: 100 },
    refreshKey,
  );
  const [form, setForm] = useState({
    name: "",
    description: "",
    lowSlaMinutes: 1440,
    mediumSlaMinutes: 480,
    highSlaMinutes: 240,
    criticalSlaMinutes: 120,
  });
  const [error, setError] = useState("");

  async function create(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(form.name, form.description)) {
      setError("Completa el nombre y la descripción de la categoría.");
      return;
    }
    try {
      await supportCategoryRepository.create(token, form);
      setForm({
        name: "",
        description: "",
        lowSlaMinutes: 1440,
        mediumSlaMinutes: 480,
        highSlaMinutes: 240,
        criticalSlaMinutes: 120,
      });
      setRefreshKey((value) => value + 1);
      notify("Categoría creada.");
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <section className="panel">
      <div className="section-head">
        <div>
          <p className="eyebrow">Catálogo</p>
          <h2>Categorías y SLA</h2>
        </div>
      </div>
      {user.role === "superAdmin" && (
        <form className="category-form" onSubmit={create} noValidate>
          <input
            placeholder="Nombre"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            required
          />
          <input
            placeholder="Descripción"
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            required
          />
          {priorities.map((priority) => (
            <label key={priority}>
              SLA {toLabel(priority)} (min)
              <input
                type="number"
                min="1"
                value={form[`${priority}SlaMinutes`]}
                onChange={(e) =>
                  setForm({
                    ...form,
                    [`${priority}SlaMinutes`]: Number(e.target.value),
                  })
                }
              />
            </label>
          ))}
          {error && <div className="error-box">{error}</div>}
          <button className="primary-button">Crear categoría</button>
        </form>
      )}
      {categories.loading && <p className="muted">Cargando categorías...</p>}
      <div className="cards-grid">
        {(categories.data?.items || []).map((category) => (
          <CategoryCard
            key={category.id}
            token={token}
            user={user}
            category={category}
            notify={notify}
            refreshKey={refreshKey}
            onChanged={() => setRefreshKey((value) => value + 1)}
          />
        ))}
      </div>
    </section>
  );
}

function CategoryCard({ token, user, category, notify, refreshKey, onChanged }) {
  const detail = useSupportCategoryDetail(token, category.id, refreshKey);
  const [sla, setSla] = useState({
    priority: "critical",
    responseTimeMinutes: 120,
  });
  const [edit, setEdit] = useState({
    name: category.name,
    description: category.description,
  });
  const [error, setError] = useState("");

  useEffect(() => {
    setEdit({ name: category.name, description: category.description });
  }, [category.name, category.description]);

  useEffect(() => {
    const selectedPolicy = (detail.data?.slaPolicies || []).find(
      (policy) => policy.priority === sla.priority,
    );
    const currentMinutes = durationToMinutes(selectedPolicy?.responseTime);
    if (currentMinutes !== null) {
      setSla((current) => ({
        ...current,
        responseTimeMinutes: currentMinutes,
      }));
    }
  }, [detail.data, sla.priority]);

  async function updateCategory(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(edit.name, edit.description)) {
      setError("Completa el nombre y la descripción de la categoría.");
      return;
    }
    try {
      await supportCategoryRepository.update(token, category.id, edit);
      notify("Categoría actualizada.");
      onChanged();
    } catch (err) {
      setError(err.message);
    }
  }

  async function toggleActive() {
    setError("");
    try {
      await supportCategoryRepository.setActive(
        token,
        category.id,
        !category.isActive,
      );
      notify(
        category.isActive ? "Categoría desactivada." : "Categoría activada.",
      );
      onChanged();
    } catch (err) {
      setError(err.message);
    }
  }

  async function updateSla(event) {
    event.preventDefault();
    setError("");
    try {
      await supportCategoryRepository.updateSla(
        token,
        category.id,
        sla.priority,
        sla.responseTimeMinutes,
      );
      notify("SLA actualizado correctamente.");
      onChanged();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <article className="data-card">
      <div className="section-head compact">
        <div>
          <h3>{category.name}</h3>
          <p>{category.description}</p>
        </div>
        <span className="status">
          {category.isActive ? "Activa" : "Inactiva"}
        </span>
      </div>
      {user.role === "superAdmin" && (
        <form
          className="mini-form category-edit-form"
          onSubmit={updateCategory}
          noValidate
        >
          <input
            value={edit.name}
            onChange={(e) => setEdit({ ...edit, name: e.target.value })}
            placeholder="Nombre"
            required
          />
          <input
            value={edit.description}
            onChange={(e) => setEdit({ ...edit, description: e.target.value })}
            placeholder="Descripción"
            required
          />
          <button className="secondary-button">Guardar</button>
          <button className="ghost-button" type="button" onClick={toggleActive}>
            {category.isActive ? "Desactivar" : "Activar"}
          </button>
        </form>
      )}
      <div className="sla-list">
        {(detail.data?.slaPolicies || []).map((policy) => (
          <span key={policy.id}>
            {toLabel(policy.priority)}: {policy.responseTime}
          </span>
        ))}
      </div>
      {isPrivileged(user.role) && (
        <form className="mini-form" onSubmit={updateSla} noValidate>
          <select
            value={sla.priority}
            onChange={(e) =>
              setSla((current) => ({
                ...current,
                priority: e.target.value,
              }))
            }
          >
            {priorities.map((priority) => (
              <option key={priority} value={priority}>
                {toLabel(priority)}
              </option>
            ))}
          </select>
          <input
            type="number"
            min="1"
            value={sla.responseTimeMinutes}
            onChange={(e) =>
              setSla({ ...sla, responseTimeMinutes: e.target.value })
            }
          />
          <button className="secondary-button">Actualizar SLA</button>
        </form>
      )}
      {error && <div className="error-box">{error}</div>}
    </article>
  );
}

