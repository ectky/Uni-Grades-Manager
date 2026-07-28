import { useState } from "react";
import { registerUser } from "../../api/authApi";
import { AppLayout } from "../../components/AppLayout";
import { ErrorBanner, extractErrorMessage } from "../../components/Common";

// NOTE: there was no screenshot/mockup for this screen in Chapter 3 — it
// wasn't part of the original UI walkthrough. It's included here because it's
// structurally required: POST /api/auth/register is Admin-only, so without
// some screen to call it, no Instructor or Student account could ever be
// created after the first Admin exists. Restyle freely; the important part
// is the API call underneath.
export function AdminUsersPage() {
  const [form, setForm] = useState({ name: "", email: "", password: "", type: "Student" });
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [submitting, setSubmitting] = useState(false);

  function update(field, value) {
    setForm((f) => ({ ...f, [field]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setSuccessMessage("");
    setSubmitting(true);
    try {
      const created = await registerUser(form);
      setSuccessMessage(`Създаден е потребител „${created.name}" (${created.role}).`);
      setForm({ name: "", email: "", password: "", type: "Student" });
    } catch (err) {
      setError(extractErrorMessage(err, "Неуспешно създаване на потребител."));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <AppLayout title="Създаване на потребител" subtitle="Администраторски панел">
      <ErrorBanner message={error} />
      {successMessage && <div className="success-banner"><span>{successMessage}</span></div>}

      <div className="panel" style={{ maxWidth: 480 }}>
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Име</label>
            <input
              required
              value={form.name}
              onChange={(e) => update("name", e.target.value)}
            />
          </div>
          <div className="field">
            <label>Имейл</label>
            <input
              type="email"
              required
              value={form.email}
              onChange={(e) => update("email", e.target.value)}
            />
          </div>
          <div className="field">
            <label>Парола</label>
            <input
              type="password"
              required
              minLength={8}
              value={form.password}
              onChange={(e) => update("password", e.target.value)}
            />
          </div>
          <div className="field">
            <label>Роля</label>
            <select value={form.type} onChange={(e) => update("type", e.target.value)}>
              <option value="Student">Student</option>
              <option value="Instructor">Instructor</option>
              <option value="Admin">Admin</option>
            </select>
          </div>
          <button className="btn btn-primary" type="submit" disabled={submitting}>
            {submitting ? "Създаване…" : "Създай потребител"}
          </button>
        </form>
      </div>
    </AppLayout>
  );
}
