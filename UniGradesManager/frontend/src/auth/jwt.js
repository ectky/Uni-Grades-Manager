import { jwtDecode } from "jwt-decode";

/**
 * Decodes the JWT issued by User Service (Chapter 4.2) to read its claims
 * client-side. This never verifies the signature — the frontend trusts the
 * token only enough to render the right UI; every actual authorization
 * decision is re-checked by whichever service receives the request.
 */
export function decodeToken(token) {
  try {
    const payload = jwtDecode(token);
    return {
      userId: Number(
        payload["nameid"] ??
          payload["sub"] ??
          payload[
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
          ]
      ),
      name:
        payload["name"] ??
        payload["unique_name"] ??
        payload[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
        ],
      role:
        payload["role"] ??
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ],
      exp: payload["exp"],
    };
  } catch {
    return null;
  }
}

export function isTokenExpired(exp) {
  if (!exp) return true;
  return Date.now() >= exp * 1000;
}
