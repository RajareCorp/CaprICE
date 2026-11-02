# Contributing

Thanks for helping improve CaprICE. Bug reports, documentation improvements, and focused pull requests are welcome.

## Before you start

- Search existing issues and pull requests to avoid duplicate work.
- For larger changes, open an issue first to agree on the scope.
- For security vulnerabilities, follow [SECURITY.md](SECURITY.md); do not open a public issue with vulnerability details.

## Build locally

CaprICE currently targets Windows and is compiled by `build-caprice.ps1`. You need Visual Studio Build Tools with `csc.exe` and the Harmony dependency at `lib\0Harmony.dll`.

```powershell
powershell -ExecutionPolicy Bypass -File .\build-caprice.ps1
```

The executable and its required `0Harmony.dll` are written to `out\`. Do not commit generated build output.

## Pull requests

1. Keep changes focused and follow existing code patterns.
2. Update the README or architecture documentation when behavior or setup changes.
3. Build the application and test the changed behavior on Windows. Include the commands run and any manual test steps in your pull request.
4. Describe the user impact, relevant issue, and any limitations. Do not include secrets, real prompts, or unredacted audit logs.

There is no automated test suite at present. A successful build is required; add focused tests when the change introduces logic that can be tested independently.

## Contributing in French

Les rapports de bugs, améliorations de documentation et pull requests ciblées sont les bienvenus.

- Vérifiez les issues et pull requests existantes. Pour un changement important, ouvrez d'abord une issue afin de convenir du périmètre.
- Pour une vulnérabilité, suivez [SECURITY.md](SECURITY.md) et ne publiez pas ses détails dans une issue publique.
- Le projet se compile actuellement sous Windows avec Visual Studio Build Tools (`csc.exe`) et `lib\0Harmony.dll` :

  ```powershell
  powershell -ExecutionPolicy Bypass -File .\build-caprice.ps1
  ```

- Avant de soumettre une pull request, compilez et testez le comportement modifié sous Windows. Décrivez les commandes et les étapes de test.
- Gardez les changements ciblés, mettez à jour la documentation concernée et ne publiez aucun secret, prompt réel ou journal d'audit non expurgé.

La suite ne dispose pas encore de tests automatisés ; une compilation réussie est requise.
