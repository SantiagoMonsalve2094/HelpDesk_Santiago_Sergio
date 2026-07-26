import React, { useState } from "react";
import { hasBlankFields } from "../../domain/validation";

export function LoginPage({ onLogin }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function submit(event) {
    event.preventDefault();
    if (hasBlankFields(email, password)) {
      setError("Ingresa el correo electrónico y la contraseña.");
      return;
    }
    setLoading(true);
    setError("");
    try {
      await onLogin({ email, password });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-panel">
        <div>
          <p className="eyebrow">Mesa de ayuda</p>
          <h1>HelpDesk</h1>
          <p className="muted">Gestión de tickets, SLA, técnicos y categorías.</p>
        </div>
        <form onSubmit={submit} className="stack" noValidate>
          <label>
            Correo electrónico
            <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          <label>
            Contraseña
            <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
          </label>
          {error && <div className="error-box">{error}</div>}
          <button className="primary-button" disabled={loading}>{loading ? "Ingresando..." : "Ingresar"}</button>
        </form>
      </section>
    </main>
  );
}
