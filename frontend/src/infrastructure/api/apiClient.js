import { normalizeErrorMessage } from "./errors";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:8080";

export async function apiRequest(path, { token, method = "GET", body, query } = {}) {
  const url = new URL(`${API_BASE_URL}${path}`);
  Object.entries(query || {}).forEach(([key, value]) => {
    if (value !== "" && value !== null && value !== undefined) {
      url.searchParams.set(key, value);
    }
  });

  let response;
  try {
    response = await fetch(url, {
      method,
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      },
      body: body ? JSON.stringify(body) : undefined
    });
  } catch {
    throw new Error("No se pudo conectar con la API. Verifica que el backend esté en ejecución.");
  }

  if (response.status === 204) return null;
  const text = await response.text();
  const data = text ? JSON.parse(text) : null;
  if (!response.ok) {
    const message = data?.errors?.[0]?.message || data?.data || data?.title || "La API rechazó la solicitud.";
    throw new Error(normalizeErrorMessage(message));
  }
  return data;
}
