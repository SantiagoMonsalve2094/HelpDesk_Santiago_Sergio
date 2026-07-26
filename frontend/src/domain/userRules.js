export function buildCreateUserPayload(form) {
  const base = {
    fullName: form.fullName,
    email: form.email,
    password: form.password,
    role: form.role
  };

  if (form.role === "technician") {
    return {
      ...base,
      supportCategoryIds: form.supportCategoryIds,
      maxActiveTickets: Number(form.maxActiveTickets)
    };
  }

  if (form.role === "supervisor") {
    return {
      ...base,
      supportCategoryIds: form.supervisorCategoryId ? [form.supervisorCategoryId] : [],
      maxActiveTickets: null
    };
  }

  return {
    ...base,
    supportCategoryIds: [],
    maxActiveTickets: null
  };
}

export function validateCreateUserForm(form) {
  if (!form.fullName?.trim() || !form.email?.trim() || !form.password?.trim() || !form.role) {
    return "Completa el nombre, el correo, la contraseña y el rol.";
  }

  if (form.role === "technician") {
    if (!Array.isArray(form.supportCategoryIds) || form.supportCategoryIds.length === 0) {
      return "Selecciona al menos una categoría para el técnico.";
    }
    if (!Number(form.maxActiveTickets) || Number(form.maxActiveTickets) < 1) {
      return "Define una capacidad máxima válida para el técnico.";
    }
  }

  if (form.role === "supervisor" && !form.supervisorCategoryId) {
    return "Selecciona exactamente una categoría para el supervisor.";
  }

  return "";
}
