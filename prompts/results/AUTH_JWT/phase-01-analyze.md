AUTH_JWT – Phase 1: Analyze (Result)

Date: 2025-10-31

Scope: Analyze Auth flow end-to-end with the new Phase 1 prompt style (functions list with 1–5 points), aligned with `AuthController` and `JwtService` in codebase.

Functions/Endpoints to test (per refined Phase 1 prompt)

1) POST /api/v1/auth/register (Register)
- Main: Create new user with username/password; hash password and persist.
- Inputs: username, password (min length/complexity), displayName? (optional).
- Returns: 200 + user info (no password) or 409 if username exists.
- Edge cases: empty/whitespace username; weak/short password; duplicate username; unicode username.
- Dependencies: Mongo users collection; BCrypt hashing; unique index on username.

2) POST /api/v1/auth/login (Login)
- Main: Verify credentials; return JWT + basic profile.
- Inputs: username, password.
- Returns: 200 + token, claims, expiresAt; 401 for invalid credentials.
- Edge cases: wrong username; wrong password; disabled/locked user (if any policy); case sensitivity.
- Dependencies: Mongo users; BCrypt Verify; JwtService.GenerateToken.

3) GET /api/v1/auth/me (Me)
- Main: Return current user profile from JWT claims.
- Inputs: Bearer token.
- Returns: 200 + profile; 401 when token missing/invalid/expired.
- Edge cases: malformed token; wrong audience/issuer; missing claims.
- Dependencies: JwtBearer middleware; JwtService validation params.

4) POST /api/v1/auth/change-password (ChangePassword)
- Main: Change current user's password after verifying old password.
- Inputs: oldPassword, newPassword (rules/complexity), Bearer token.
- Returns: 200; 400/401 if old password mismatch; 401 if token invalid.
- Edge cases: newPassword = oldPassword; weak password; empty fields; concurrency (recent login only?)
- Dependencies: Mongo users; BCrypt hash/verify; Jwt authentication context.

5) JwtService.GenerateToken(user)
- Main: Create JWT with correct issuer/audience/expiry and claims.
- Inputs: key, issuer, audience, expiration (from config), user claims.
- Returns: compact JWT string.
- Edge cases: missing key; invalid issuer/audience; very short/long expiry; unicode in claims.
- Dependencies: JwtOptions in Program.cs; Microsoft.IdentityModel.Tokens.

Assumptions & Invariants
- Passwords stored only as BCrypt hashes; no plaintext.
- Username uniqueness enforced; case sensitivity clarified by tests.
- JWT includes sub (userId), username, role(s) if any; HS256 signing.
- No seed data; tests should be idempotent and assert via structure/status.

Risk Register
- Security: weak passwords, duplicate accounts, token forgery, audience/issuer mismatch.
- Consistency: race on register, atomic password change, claims completeness.
- Performance: login burst, token generation cost negligible.

Categories
- HappyPath, EdgeCase, ErrorHandling, Security, Performance.

Exit Criteria
- Function list analyzed with 1–5 points; risks and categories aligned with `tests/AUTH_JWT_TEST_PLAN.md`.
# AUTH_JWT – Phase 1: Analyze (Result)

- Endpoints: register, login, me, change-password
- Claims: sub, username, email; issuer/audience (if enforced)
- Risks: duplicate user, weak passwords, token misuse
