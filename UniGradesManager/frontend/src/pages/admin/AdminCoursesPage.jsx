import { useEffect, useState } from "react";
import { createCourse, getAllCourses } from "../../api/coursesApi";
import { AppLayout } from "../../components/AppLayout";
import { ErrorBanner, LoadingSpinner, extractErrorMessage } from "../../components/Common";

// Also not part of the original Chapter 3 walkthrough, for the same reason as
// AdminUsersPage: POST /api/courses is Admin-only, and Parser Service only
// ever checks that a course already exists — it can't create one on the fly
// (the Excel file has no Name/Period columns). Something has to call this
// endpoint, so here it is.
export function AdminCoursesPage() {
  const [courses, setCourses] = useState(null);
  const [error, setError] = useState("");
  const [form, setForm] = useState({ name: "", period: "", instructorId: "" });
  const [submitting, setSubmitting] = useState(false);

  function reload() {
    getAllCourses()
      .then(setCourses)
      .catch((err) => setError(extractErrorMessage(err)));
  }

  useEffect(reload, []);

  function update(field, value) {
    setForm((f) => ({ ...f, [field]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setSubmitting(true);
    try {
      await createCourse({
        name: form.name,
        period: form.period,
        instructorId: Number(form.instructorId),
      });
      setForm({ name: "", period: "", instructorId: "" });
      reload();
    } catch (err) {
      setError(extractErrorMessage(err, "Неуспешно създаване на курс."));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <AppLayout title="Курсове" subtitle="Администраторски панел">
      <ErrorBanner message={error} />

      <div className="content-row">
        <div>
          {!courses ? (
            <LoadingSpinner />
          ) : (
            <table className="data-table">
              <thead>
                <tr>
                  <td>Курс</td>
                  <td>Период</td>
                  <td>Instructor ID</td>
                </tr>
              </thead>
              <tbody>
                {courses.map((c) => (
                  <tr key={c.id}>
                    <td>{c.name}</td>
                    <td>{c.period}</td>
                    <td>{c.instructorId}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <div className="panel">
          <h3>Нов курс</h3>
          <form onSubmit={handleSubmit}>
            <div className="field">
              <label>Наименование</label>
              <input required value={form.name} onChange={(e) => update("name", e.target.value)} />
            </div>
            <div className="field">
              <label>Период</label>
              <input
                required
                placeholder="напр. Летен 2026"
                value={form.period}
                onChange={(e) => update("period", e.target.value)}
              />
            </div>
            <div className="field">
              <label>Instructor ID</label>
              <input
                required
                type="number"
                min={1}
                value={form.instructorId}
                onChange={(e) => update("instructorId", e.target.value)}
              />
            </div>
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? "Създаване…" : "Създай курс"}
            </button>
          </form>
        </div>
      </div>
    </AppLayout>
  );
}
