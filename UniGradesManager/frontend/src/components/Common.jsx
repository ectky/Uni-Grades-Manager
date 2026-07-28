export function StatCard({ label, value, tone }) {
  return (
    <div className="card">
      <div className="label">{label}</div>
      <div className={"value" + (tone ? ` ${tone}` : "")}>{value}</div>
    </div>
  );
}

export function LoadingSpinner({ label = "Зареждане…" }) {
  return <div className="centered-loading">{label}</div>;
}

export function ErrorBanner({ message }) {
  if (!message) return null;
  return <div className="error-banner">{message}</div>;
}

/** Extracts a human-readable message from an axios error / ProblemDetails response. */
export function extractErrorMessage(error, fallback = "Възникна грешка. Опитайте отново.") {
  return (
    error?.response?.data?.detail ??
    error?.response?.data?.title ??
    error?.message ??
    fallback
  );
}
