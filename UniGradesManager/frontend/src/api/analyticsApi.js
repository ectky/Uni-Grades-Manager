import { analyticsServiceClient } from "./httpClient";

/** GET /api/analytics/course/{id} — All roles. Summary stats for one course. */
export async function getCourseStats(courseId) {
  const { data } = await analyticsServiceClient.get(
    `/api/analytics/course/${courseId}`
  );
  return data; // { average, median, mode, stdDev, min, max, passRatePercent, studentCount }
}

/** GET /api/analytics/course/{id}/distribution — All roles. Bars for the chart. */
export async function getCourseDistribution(courseId) {
  const { data } = await analyticsServiceClient.get(
    `/api/analytics/course/${courseId}/distribution`
  );
  return data; // [{ grade: 2, count: 5 }, { grade: 3, count: 9 }, ...]
}

/**
 * GET /api/analytics/course/{id}/grades — Instructor, Admin.
 * Per-student grades for a course, already enriched with student names
 * (Analytics Service calls User Service internally for this).
 */
export async function getCourseGrades(courseId) {
  const { data } = await analyticsServiceClient.get(
    `/api/analytics/course/${courseId}/grades`
  );
  return data; // [{ studentId, studentName, value }, ...]
}

/**
 * GET /api/analytics/student/me — Student only.
 * The logged-in student's own aggregate (average, count of graded courses).
 * No id passed — User.Id (JWT identity) and User.StudentId (roll number,
 * what Grades Service actually stores against) are different numbers in
 * this system, so "my stats" resolves the roll number server-side instead
 * of the frontend needing to know or guess it.
 */
export async function getMyStudentStats() {
  const { data } = await analyticsServiceClient.get("/api/analytics/student/me");
  return data; // { average, gradedCourseCount }
}

/**
 * GET /api/analytics/student/me/grades — Student only.
 * The logged-in student's own per-course grades, enriched with course
 * name/period. Same reasoning as getMyStudentStats — no id needed.
 */
export async function getMyStudentGrades() {
  const { data } = await analyticsServiceClient.get("/api/analytics/student/me/grades");
  return data; // [{ courseId, courseName, period, value }, ...]
}

/**
 * GET /api/analytics/student/{rollNumber} — Instructor only.
 * Looks up a specific student by roll number (User.StudentId) — NOT a
 * User.Id. Useful if an instructor already knows a student's roll number
 * from a roster; there's no reverse lookup from name to roll number here.
 */
export async function getStudentStatsByRollNumber(rollNumber) {
  const { data } = await analyticsServiceClient.get(
    `/api/analytics/student/${rollNumber}`
  );
  return data;
}

/** GET /api/analytics/student/{rollNumber}/grades — Instructor only. Same roll-number caveat as above. */
export async function getStudentGradesByRollNumber(rollNumber) {
  const { data } = await analyticsServiceClient.get(
    `/api/analytics/student/${rollNumber}/grades`
  );
  return data;
}