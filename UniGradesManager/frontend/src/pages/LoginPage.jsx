import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../components/Common";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setSubmitting(true);
    try {
      await login(email, password);
      const redirectTo = location.state?.from?.pathname ?? "/";
      navigate(redirectTo, { replace: true });
    } catch (err) {
      // User Service returns the same generic message whether the email or
      // the password was wrong (Chapter 4.5) — shown here as-is, unchanged.
      setError(extractErrorMessage(err, "Невалиден имейл или парола."));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="login-screen">
      <div className="login-card">
        <div className="logo">EduGrade</div>
        <div className="sub">Вход в системата</div>

        {error && <div className="login-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="email">Имейл</label>
            <input
              id="email"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              autoComplete="username"
            />
          </div>
          <div className="field">
            <label htmlFor="password">Парола</label>
            <input
              id="password"
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
            />
          </div>
          <button className="btn btn-primary" type="submit" disabled={submitting} style={{ width: "100%" }}>
            {submitting ? "Влизане…" : "Вход"}
          </button>
        </form>

        <div className="footer-note">
          Достъп само с университетски имейл и парола — акаунтите се създават от администратор.
        </div>
      </div>
    </div>
  );
}
