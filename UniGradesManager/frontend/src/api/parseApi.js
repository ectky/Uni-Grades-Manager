import { parserServiceClient } from "./httpClient";

/**
 * POST /api/parse — Instructor only. Multipart form with the target
 * CourseId plus the .xlsx file. Parser Service validates the file, confirms
 * the course exists, and writes the grades — this single call does all of
 * it (there's no separate "confirm" endpoint; the confirmation screen just
 * displays this response, it doesn't trigger a second write).
 */
export async function uploadGrades(courseId, file) {
  const form = new FormData();
  form.append("CourseId", courseId);
  form.append("File", file);

  const { data } = await parserServiceClient.post("/api/parse", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data; // ParseResultDto: courseId, courseName, period, recordCount, processingTimeMs, records[]
}

/** GET /api/parse/template — Instructor only. Triggers a browser download. */
export async function downloadGradeTemplate() {
  const response = await parserServiceClient.get("/api/parse/template", {
    responseType: "blob",
  });

  const url = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement("a");
  link.href = url;
  link.setAttribute("download", "grades_template.xlsx");
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}
