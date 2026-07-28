import { userServiceClient } from "./httpClient";

/** POST /api/auth/login — Public. */
export async function login(email, password) {
  const { data } = await userServiceClient.post("/api/auth/login", {
    email,
    password,
  });
  return data; // { token, userId, name, role }
}

/** POST /api/auth/register — Admin only. */
export async function registerUser({ name, email, password, type, studentId }) {
  const { data } = await userServiceClient.post("/api/auth/register", {
    name,
    email,
    password,
    type,
    studentId: studentId ?? null,
  });
  return data;
}

/** GET /api/users/{id} — Admin, Owner. */
export async function getUserProfile(id) {
  const { data } = await userServiceClient.get(`/api/users/${id}`);
  return data;
}
