import { coursesServiceClient } from "./httpClient";

/** GET /api/courses — Admin only. */
export async function getAllCourses() {
  const { data } = await coursesServiceClient.get("/api/courses");
  return data;
}

/** GET /api/courses/my — Instructor only. */
export async function getMyCourses() {
  const { data } = await coursesServiceClient.get("/api/courses/my");
  return data;
}

/** POST /api/courses — Admin only. */
export async function createCourse({ name, period, instructorId }) {
  const { data } = await coursesServiceClient.post("/api/courses", {
    name,
    period,
    instructorId,
  });
  return data;
}

/** PUT /api/courses/{id} — Admin, or the owning Instructor. */
export async function updateCourse(id, { name, period }) {
  const { data } = await coursesServiceClient.put(`/api/courses/${id}`, {
    name,
    period,
  });
  return data;
}
