export function normalizeErrorMessage(message) {
  const fallback = "No se pudo completar la operación.";
  if (!message) return fallback;

  const translations = {
    "Failed to fetch": "No se pudo conectar con la API. Verifica que el backend esté en ejecución.",
    "NetworkError when attempting to fetch resource.": "No se pudo conectar con la API. Verifica que el backend esté en ejecución.",
    "Load failed": "No se pudo conectar con la API. Verifica que el backend esté en ejecución.",
    "The user name or password is incorrect.": "El correo o la contraseña son incorrectos.",
    "Invalid credentials.": "El correo o la contraseña son incorrectos.",
    Unauthorized: "No tienes una sesión válida. Inicia sesión nuevamente.",
    Forbidden: "No tienes permisos para realizar esta acción.",
    "Not Found": "No se encontró el recurso solicitado."
  };

  return translations[message] || message || fallback;
}
