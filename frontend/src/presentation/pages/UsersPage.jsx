import React, { useEffect, useState } from "react";
import { roles } from "../../domain/constants";
import { toLabel } from "../../domain/formatters";
import { buildCreateUserPayload, validateCreateUserForm } from "../../domain/userRules";
import { hasBlankFields } from "../../domain/validation";
import { useSupportCategories } from "../../application/hooks/useCategories";
import { useUsers, useUserDetail, userRepository } from "../../application/hooks/useUsers";
export function UsersPage({ token, notify }) {
  const [refreshKey, setRefreshKey] = useState(0);
  const users = useUsers(token, { pageSize: 100 }, refreshKey);
  const categories = useSupportCategories(token, {
    includeInactive: true,
    pageSize: 100,
  });
  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    role: "user",
    supportCategoryIds: [],
    supervisorCategoryId: "",
    maxActiveTickets: 5,
  });
  const [error, setError] = useState("");
  const activeCategories = (categories.data?.items || []).filter(
    (category) => category.isActive,
  );

  async function create(event) {
    event.preventDefault();
    setError("");
    const validationError = validateCreateUserForm(form);
    if (validationError) {
      setError(validationError);
      return;
    }
    try {
      await userRepository.create(token, buildCreateUserPayload(form));
      setForm({
        fullName: "",
        email: "",
        password: "",
        role: "user",
        supportCategoryIds: [],
        supervisorCategoryId: "",
        maxActiveTickets: 5,
      });
      setRefreshKey((value) => value + 1);
      notify("Usuario creado.");
    } catch (err) {
      setError(err.message);
    }
  }

  async function toggleActive(userItem) {
    setError("");
    try {
      await userRepository.setActive(token, userItem.id, !userItem.isActive);
      setRefreshKey((value) => value + 1);
      notify(userItem.isActive ? "Usuario desactivado." : "Usuario activado.");
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <section className="panel">
      <div className="section-head">
        <div>
          <p className="eyebrow">{"Administraci\u00f3n"}</p>
          <h2>Usuarios</h2>
        </div>
      </div>
      <form className="category-form" onSubmit={create} noValidate>
        <input
          placeholder="Nombre completo"
          value={form.fullName}
          onChange={(e) => setForm({ ...form, fullName: e.target.value })}
          required
        />
        <input
          placeholder={"Correo electr\u00f3nico"}
          type="email"
          value={form.email}
          onChange={(e) => setForm({ ...form, email: e.target.value })}
          required
        />
        <input
          placeholder={"Contrase\u00f1a"}
          type="password"
          value={form.password}
          onChange={(e) => setForm({ ...form, password: e.target.value })}
          required
        />
        <select
          value={form.role}
          onChange={(e) =>
            setForm({
              ...form,
              role: e.target.value,
              supportCategoryIds: [],
              supervisorCategoryId: "",
              maxActiveTickets: 5,
            })
          }
        >
          {roles.map((role) => (
            <option key={role} value={role}>
              {toLabel(role)}
            </option>
          ))}
        </select>
        {form.role === "technician" && (
          <select
            multiple
            value={form.supportCategoryIds}
            onChange={(e) =>
              setForm({
                ...form,
                supportCategoryIds: Array.from(
                  e.target.selectedOptions,
                  (option) => option.value,
                ),
              })
            }
          >
            {activeCategories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        )}
        {form.role === "supervisor" && (
          <select
            value={form.supervisorCategoryId}
            onChange={(e) =>
              setForm({ ...form, supervisorCategoryId: e.target.value })
            }
          >
            <option value="">{"Categor\u00eda supervisada"}</option>
            {activeCategories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        )}
        {form.role === "technician" && (
          <label className="capacity-field">
            {"Capacidad m\u00e1xima de tickets"}
            <input
              type="number"
              min="1"
              value={form.maxActiveTickets}
              onChange={(e) =>
                setForm({ ...form, maxActiveTickets: Number(e.target.value) })
              }
            />
          </label>
        )}
        {error && <div className="error-box">{error}</div>}
        <button className="primary-button">Crear usuario</button>
      </form>
      <div className="table users-table">
        <div className="table-row header">
          <span>Nombre</span>
          <span>Correo</span>
          <span>Rol</span>
          <span>Activo</span>
          <span>Acciones</span>
        </div>
        {(users.data?.items || []).map((item) => (
          <UserRow
            key={item.id}
            token={token}
            userItem={item}
            categories={categories.data?.items || []}
            onChanged={() => setRefreshKey((value) => value + 1)}
            onToggleActive={toggleActive}
            notify={notify}
          />
        ))}
      </div>
    </section>
  );
}

function UserRow({
  token,
  userItem,
  categories,
  onChanged,
  onToggleActive,
  notify,
}) {
  const [expanded, setExpanded] = useState(false);
  const [detailRefresh, setDetailRefresh] = useState(0);
  const detail = useUserDetail(token, userItem.id, expanded, detailRefresh);

  function refresh(message) {
    if (message) notify(message);
    onChanged();
    setDetailRefresh((value) => value + 1);
  }

  return (
    <>
      <div className="table-row">
        <span>{userItem.fullName}</span>
        <span>{userItem.email}</span>
        <span>{toLabel(userItem.role)}</span>
        <span>{userItem.isActive ? "Sí" : "No"}</span>
        <span className="row-actions">
          <button
            className="ghost-button table-action"
            onClick={() => setExpanded((value) => !value)}
          >
            {expanded ? "Ocultar" : "Detalle"}
          </button>
          <button
            className="ghost-button table-action"
            onClick={() => onToggleActive(userItem)}
          >
            {userItem.isActive ? "Desactivar" : "Activar"}
          </button>
        </span>
      </div>
      {expanded && (
        <div className="user-detail-row">
          {detail.loading && <p className="muted">Cargando usuario...</p>}
          {detail.error && <div className="error-box">{detail.error}</div>}
          {detail.data && (
            <UserDetailForms
              token={token}
              userDetail={detail.data}
              categories={categories}
              onChanged={refresh}
            />
          )}
        </div>
      )}
    </>
  );
}

function UserDetailForms({ token, userDetail, categories, onChanged }) {
  const [identity, setIdentity] = useState({
    fullName: userDetail.fullName,
    email: userDetail.email,
  });
  const [password, setPassword] = useState("");
  const [technicianProfile, setTechnicianProfile] = useState({
    supportCategoryIds: userDetail.technicianProfile?.supportCategoryIds || [],
    maxActiveTickets: userDetail.technicianProfile?.maxActiveTickets || 5,
  });
  const [supervisorCategoryId, setSupervisorCategoryId] = useState(
    userDetail.supervisorProfile?.supportCategoryId || "",
  );
  const [error, setError] = useState("");
  const activeCategories = categories.filter((category) => category.isActive);

  useEffect(() => {
    setIdentity({ fullName: userDetail.fullName, email: userDetail.email });
    setPassword("");
    setTechnicianProfile({
      supportCategoryIds:
        userDetail.technicianProfile?.supportCategoryIds || [],
      maxActiveTickets: userDetail.technicianProfile?.maxActiveTickets || 5,
    });
    setSupervisorCategoryId(
      userDetail.supervisorProfile?.supportCategoryId || "",
    );
    setError("");
  }, [userDetail]);

  async function updateIdentity(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(identity.fullName, identity.email)) {
      setError("Completa el nombre y el correo.");
      return;
    }
    try {
      await userRepository.updateIdentity(token, userDetail.id, identity);
      onChanged("Identidad actualizada.");
    } catch (err) {
      setError(err.message);
    }
  }

  async function resetPassword(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(password)) {
      setError("Ingresa la nueva contraseña.");
      return;
    }
    try {
      await userRepository.resetPassword(token, userDetail.id, password);
      setPassword("");
      onChanged("Contraseña actualizada.");
    } catch (err) {
      setError(err.message);
    }
  }

  async function updateTechnicianProfile(event) {
    event.preventDefault();
    setError("");
    if (
      hasBlankFields(
        technicianProfile.supportCategoryIds,
        technicianProfile.maxActiveTickets,
      )
    ) {
      setError("Selecciona categorías y define la capacidad máxima.");
      return;
    }
    try {
      await userRepository.updateTechnicianProfile(
        token,
        userDetail.id,
        technicianProfile,
      );
      onChanged("Perfil técnico actualizado.");
    } catch (err) {
      setError(err.message);
    }
  }

  async function updateSupervisorCategory(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(supervisorCategoryId)) {
      setError("Selecciona una categoría para el supervisor.");
      return;
    }
    try {
      await userRepository.updateSupervisorCategory(
        token,
        userDetail.id,
        supervisorCategoryId,
      );
      onChanged("Categoría de supervisor actualizada.");
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div className="user-detail-grid">
      {error && <div className="error-box">{error}</div>}
      <form className="inline-form" onSubmit={updateIdentity} noValidate>
        <strong>Identidad</strong>
        <input
          value={identity.fullName}
          onChange={(e) =>
            setIdentity({ ...identity, fullName: e.target.value })
          }
          placeholder="Nombre completo"
          required
        />
        <input
          value={identity.email}
          onChange={(e) => setIdentity({ ...identity, email: e.target.value })}
          placeholder="Correo electrónico"
          required
        />
        <button className="secondary-button">Guardar identidad</button>
      </form>
      <form className="inline-form" onSubmit={resetPassword} noValidate>
        <strong>Contraseña</strong>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Nueva contraseña"
          required
        />
        <button className="secondary-button">Cambiar contraseña</button>
      </form>
      {userDetail.role === "technician" && (
        <form
          className="inline-form"
          onSubmit={updateTechnicianProfile}
          noValidate
        >
          <strong>Perfil técnico</strong>
          <select
            multiple
            value={technicianProfile.supportCategoryIds}
            onChange={(e) =>
              setTechnicianProfile({
                ...technicianProfile,
                supportCategoryIds: Array.from(
                  e.target.selectedOptions,
                  (option) => option.value,
                ),
              })
            }
          >
            {activeCategories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
          <label className="capacity-field">
            {"Capacidad m\u00e1xima de tickets"}
            <input
              type="number"
              min="1"
              value={technicianProfile.maxActiveTickets}
              onChange={(e) =>
                setTechnicianProfile({
                  ...technicianProfile,
                  maxActiveTickets: Number(e.target.value),
                })
              }
            />
          </label>
          <button className="secondary-button">Guardar perfil técnico</button>
        </form>
      )}
      {userDetail.role === "supervisor" && (
        <form
          className="inline-form"
          onSubmit={updateSupervisorCategory}
          noValidate
        >
          <strong>Categoría supervisada</strong>
          <select
            value={supervisorCategoryId}
            onChange={(e) => setSupervisorCategoryId(e.target.value)}
            required
          >
            <option value="">Categoría</option>
            {activeCategories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
          <button className="secondary-button">Guardar categoría</button>
        </form>
      )}
    </div>
  );
}
