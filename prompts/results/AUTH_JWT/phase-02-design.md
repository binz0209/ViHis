AUTH_JWT – Phase 2: Design (Result)

Date: 2025-10-31

Artifacts
- Test Matrix: ≥40 GWT cases across Register/Login/Me/ChangePassword + JwtService.
- Traits Standard:
  - [Trait("Feature", "AUTH_JWT")]
  - [Trait("Category", "HappyPath|EdgeCase|ErrorHandling|Security|Performance")]
  - [Trait("Priority", "P0|P1|P2")]
  - [Trait("Integration", "Real")]

Global Phase 2 Prompt (GWT) Used — NEW (standardized)
```
Generate comprehensive unit/integration test cases using Given–When–Then for the following functions:

[Register, Login, Me, ChangePassword, JwtService.GenerateToken]

For each function, include:
- Happy path scenarios
- Edge cases (boundary values)
- Error scenarios
- Integration/state interaction (where applicable)

Constraints:
- Real API only | No seed | No mock
- Add Traits: Feature, Category, Priority, Integration
- Prefer pattern-based assertions for idempotence
```

Full Matrix (≥40 cases)

Register (P0/P1)
1. GIVEN new username+password; WHEN register; THEN 200 and user returned (no password).
2. GIVEN duplicate username; WHEN register; THEN 409.
3. GIVEN empty username; WHEN register; THEN 400.
4. GIVEN whitespace username; WHEN register; THEN 400.
5. GIVEN weak password (too short); WHEN register; THEN 400.
6. GIVEN unicode username; WHEN register; THEN success.
7. GIVEN rapid double submit; WHEN register twice; THEN second 409.
8. GIVEN unexpected fields; WHEN register; THEN ignored.
9. GIVEN very long displayName; WHEN register; THEN handled (truncate/400).
10. GIVEN case-different usernames; WHEN register; THEN clarify policy (same/diff).

Login (P0/P1)
11. GIVEN correct credentials; WHEN login; THEN 200 + token + claims.
12. GIVEN wrong username; WHEN login; THEN 401.
13. GIVEN wrong password; WHEN login; THEN 401.
14. GIVEN empty username/password; WHEN login; THEN 400.
15. GIVEN unicode username; WHEN login; THEN success if registered.
16. GIVEN multiple quick attempts; WHEN login; THEN consistent responses (no lock by default).
17. GIVEN tampered body; WHEN login; THEN 400.
18. GIVEN very long password; WHEN login; THEN success if valid.

Me (P0/P1)
19. GIVEN valid token; WHEN me; THEN returns profile with expected claims.
20. GIVEN missing token; WHEN me; THEN 401.
21. GIVEN malformed token; WHEN me; THEN 401.
22. GIVEN wrong audience; WHEN me; THEN 401.
23. GIVEN wrong issuer; WHEN me; THEN 401.
24. GIVEN expired token; WHEN me; THEN 401.

Change Password (P0/P1)
25. GIVEN valid token + correct oldPassword; WHEN change; THEN 200 and can login with new.
26. GIVEN wrong oldPassword; WHEN change; THEN 400/401.
27. GIVEN weak newPassword; WHEN change; THEN 400.
28. GIVEN same newPassword as old; WHEN change; THEN 400.
29. GIVEN missing token; WHEN change; THEN 401.
30. GIVEN malformed token; WHEN change; THEN 401.

JwtService / Token Semantics (Security) (P1)
31. GIVEN valid user; WHEN GenerateToken; THEN token decodes with sub/username.
32. GIVEN missing key; WHEN GenerateToken; THEN throws.
33. GIVEN custom audience; WHEN GenerateToken; THEN validated in Me.
34. GIVEN very short expiry; WHEN Me after expiry; THEN 401.
35. GIVEN tampered signature; WHEN Me; THEN 401.

Performance/Resilience (P2)
36. GIVEN 5 parallel logins; WHEN run; THEN all succeed under soft budget.
37. GIVEN 5 parallel register with same username; WHEN run; THEN exactly one success.
38. GIVEN 10 parallel Me with valid token; WHEN run; THEN stable 200.

Data Integrity / Consistency (P2)
39. GIVEN register; WHEN login; THEN password hash stored (not plaintext) verified by re-login.
40. GIVEN change-password; WHEN old token used; THEN behavior defined (still valid/invalid by config) — assert documented behavior.
41. GIVEN login; WHEN Me; THEN claims consistent with registered user.
42. GIVEN multiple change-passwords; WHEN login; THEN only latest password works.

Exit Criteria
- ≥40 test cases designed in GWT using the new prompt; ready for Phase 3 (tests implementation).
# AUTH_JWT – Phase 2: Design (Result)

- Test plan reference: `tests/AUTH_JWT_TEST_PLAN.md` (41+ cases)
- Traits: Feature=AUTH_JWT, Integration=Real, Category, Priority
