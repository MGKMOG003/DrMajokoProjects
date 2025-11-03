# Configuring a Basic CI/CD Workflow

This guide sets up a minimal, production-ready CI for DrMajokoProjects with GitHub Actions.

## 1) Repository Structure (suggested)
```text
/ (repo root)
├─ src/
│  ├─ Web/                 # ASP.NET Core MVC (UI)
│  └─ Api/                 # ASP.NET Core Web API
├─ tests/                  # xUnit/NUnit tests (optional)
├─ docs/
│  ├─ diagrams/            # Mermaid/PlantUML
│  └─ screenshots/         # UI screenshots
├─ .github/
│  └─ workflows/
│     └─ ci.yml            # Build/Test pipeline
├─ .gitignore
├─ .gitattributes
└─ README.md
```

## 2) Add Workflow
The CI workflow at `.github/workflows/ci.yml` builds on every push/PR to `main`/`master` using .NET 9, caches NuGet, and executes tests if a `*.Tests.csproj` exists. Artifacts are uploaded to **Actions → run → Artifacts**.

## 3) Secrets (for future CD)
For future deployments (e.g., to Azure/GCP), create repo **Settings → Secrets and variables → Actions**:
- `CLOUD_PROJECT_ID`
- `CLOUD_SERVICE_KEY` (Base64‑encoded JSON key if needed)
- `FIREBASE_PROJECT_ID` (if used in CI)
- `FIREBASE_CREDENTIALS_JSON` (avoid; prefer inject at runtime in env-specific deploy step)

## 4) Local Development
```bash
dotnet restore
dotnet build
dotnet run --project src/Web/Sustainacore.Web
```

Set user secrets for dev:
```bash
dotnet user-secrets set "Firebase:ProjectId" "your-project-id"
dotnet user-secrets set "Firebase:CredentialsPath" "absolute/path/to/credentials.json"
```

## 5) Optional: CodeQL
The workflow references a reusable CodeQL template. To enable, ensure your repo has **GitHub Advanced Security** or public repo eligibility. Alternatively, remove the `codeql` job block.

## 6) Extending to CD
- Add a `deploy` job that runs after `build-test` and targets a specific environment.
- Protect production with `environment` approvals.
- For container deploys, add steps:
  ```yaml
  - name: Build container
    run: |
      docker build -t ghcr.io/${{ github.repository }}/sustainacore-api:$(git rev-parse --short HEAD) src/Api
  - name: Login GHCR
    uses: docker/login-action@v3
    with:
      registry: ghcr.io
      username: ${{ github.actor }}
      password: ${{ secrets.GITHUB_TOKEN }}
  - name: Push image
    run: docker push ghcr.io/${{ github.repository }}/sustainacore-api:$(git rev-parse --short HEAD)
  ```

## 7) Troubleshooting
- Ensure .NET SDK versions in solution match the CI matrix.
- If tests are absent, the workflow will skip them and still pass.
- Use `actions/cache` keys that include `**/*.csproj` hashes to avoid stale caches after project changes.