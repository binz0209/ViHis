# AUTH_JWT – Phase 3: Code (Result)

## What was implemented
- Integration tests added in `BackEnd/VietHistory.AI.Tests/AUTH_JWT_IntegrationTests.cs`:
  - Register duplicate username/email → 400
  - Register unicode username → 200
  - Login success, wrong password/username → 200/401
  - Login empty fields, whitespace username → 401 (current behavior)
  - Me with valid claims → 200; missing token/claims → 401; deleted user → 404
  - Change password happy path → login with new password OK
  - Change password wrong old password → 400; missing token → 404 (current behavior)
  - Parallel login consistency

- Unit tests added in `BackEnd/VietHistory.AI.Tests/AUTH_JWT_UnitTests.cs`:
  - Token contains standard claims (sub, unique_name, email, jti)
  - ValidateToken tampered/expired → null
  - ValidateToken wrong audience/issuer → null
  - ValidateToken before expiry → not null

## How to run only AUTH tests
```bash
cd BackEnd
dotnet test --filter "Feature=AUTH_JWT" -v minimal
```

## Generate coverage HTML
```bash
cd BackEnd
dotnet test --filter "Feature=AUTH_JWT" --collect:"XPlat Code Coverage" -v minimal || true
export PATH="$PATH:$HOME/.dotnet/tools"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport-auth" "-reporttypes:Html;HtmlSummary"
open coveragereport-auth/index.html
```

## Notes
- Some tests document current behavior (e.g., missing token in change-password returns 404 instead of 401). If policy changes, update controller validation and assertions accordingly.
