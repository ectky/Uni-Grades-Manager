import axios from "axios";

const TOKEN_STORAGE_KEY = "edugrade.token";

export function getStoredToken() {
  return localStorage.getItem(TOKEN_STORAGE_KEY);
}

export function setStoredToken(token) {
  localStorage.setItem(TOKEN_STORAGE_KEY, token);
}

export function clearStoredToken() {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
}

/**
 * Creates one axios instance per microservice base URL. There is no API
 * Gateway in this system (Chapter 2.3) — the frontend talks to each service
 * directly, and each service validates the JWT itself. Grades Service has no
 * client here on purpose: it has no public endpoints at all, everything
 * grade-related is read through Analytics Service.
 */
function createServiceClient(baseURL) {
  const client = axios.create({ baseURL });

  client.interceptors.request.use((config) => {
    const token = getStoredToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response?.status === 401) {
        // Token missing/expired/invalid — every service returns 401 the same
        // way since each validates independently (no shared gateway session).
        clearStoredToken();
        window.location.assign("/login");
      }
      return Promise.reject(error);
    }
  );

  return client;
}

export const userServiceClient = createServiceClient(
  import.meta.env.VITE_USER_SERVICE_URL
);
export const coursesServiceClient = createServiceClient(
  import.meta.env.VITE_COURSES_SERVICE_URL
);
export const parserServiceClient = createServiceClient(
  import.meta.env.VITE_PARSER_SERVICE_URL
);
export const analyticsServiceClient = createServiceClient(
  import.meta.env.VITE_ANALYTICS_SERVICE_URL
);
