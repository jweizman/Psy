# The Serene Path

Plateforme d'aide psychologique : homepage, chat de matching (LLM) et interface d'administration
des psychologues.

## Architecture

- **backend/** — ASP.NET Core 8 Web API (minimal API), EF Core + SQLite, intégration API
  Anthropic Claude pour le matching.
- **frontend/** — React 19 + Vite + TypeScript + Tailwind CSS.

## Prérequis

- .NET 8 SDK
- Node 20+
- Une clé API Anthropic pour la fonctionnalité de chat

## Démarrage

### Backend

```bash
cd backend
export ANTHROPIC_API_KEY=sk-ant-xxx   # ou via appsettings.json > Anthropic:ApiKey
dotnet run
# API sur http://localhost:5080
# Swagger UI sur http://localhost:5080/swagger (en dev)
```

La base SQLite `psy.db` est créée au premier lancement et pré-seedée avec 3 psychologues.

### Frontend

```bash
cd frontend
npm install
npm run dev
# UI sur http://localhost:5173
```

Pour cibler une autre URL backend : `VITE_API_URL=http://localhost:5080 npm run dev`.

## API

| Méthode | Route                         | Rôle                                      |
| ------- | ----------------------------- | ----------------------------------------- |
| GET     | `/api/psychologists?q=...`    | Liste (filtre texte sur nom/bio/skills)   |
| GET     | `/api/psychologists/{id}`     | Fiche détaillée                           |
| POST    | `/api/psychologists`          | Création (admin)                          |
| PUT     | `/api/psychologists/{id}`     | Mise à jour (admin)                       |
| DELETE  | `/api/psychologists/{id}`     | Suppression (admin)                       |
| POST    | `/api/chat`                   | Chat de matching (voir protocole)         |
| GET     | `/api/health`                 | Health check                              |

### Protocole du chat (`POST /api/chat`)

Requête :

```json
{
  "history": [{ "role": "user", "content": "Je n'arrive plus à dormir..." }],
  "phase": "initial"
}
```

Phases gérées côté backend :

1. `initial` → Claude **reformule** la problématique et demande confirmation
   → retourne `phase = "awaiting_confirmation"`.
2. `awaiting_confirmation` → classifie la réponse (yes/no) :
   - yes → lance le matching sur les bios/tags
   - non → reformule à nouveau
3. Matching : si un bon candidat existe, retourne son id + justification (`phase = "done"`).
   Sinon, propose d'ajouter des précisions (`phase = "awaiting_followup"`).
4. `awaiting_followup` → relance le matching ou clôt la session.

Le frontend est stateless : il renvoie `history` complet + `phase` courante à chaque tour.

## Routes front

- `/` — homepage (liste des psychologues disponibles)
- `/chat` — chat de matching
- `/admin` — CRUD des psychologues

## Déploiement IIS (Windows Server)

### Prérequis serveur
- Rôle **IIS** installé
- Module **URL Rewrite 2.x** (pour la règle SPA du frontend)
- **.NET 8 Hosting Bundle** (pas seulement le SDK) — enregistre
  `AspNetCoreModuleV2` dans IIS. Faire `iisreset` après installation.

### Backend (`backend/`)
```bash
dotnet publish -c Release -o C:\inetpub\psy-api
```
- Le fichier `backend/web.config` est transformé automatiquement par le SDK
  et copié dans la sortie. Éditer la sortie pour remplacer `REPLACE_ME`
  par la vraie clé Anthropic (ou définir `Anthropic:ApiKey` dans
  `appsettings.Production.json`).
- Dans IIS Manager : créer un Application Pool **No Managed Code**, puis
  un Site pointant sur `C:\inetpub\psy-api`.
- Donner les droits **Read/Write** à `IIS AppPool\<NomDuPool>` sur ce
  dossier (sinon SQLite ne peut pas créer `psy.db`).
- En cas d'erreur 500.30, passer `stdoutLogEnabled="true"` dans
  `web.config` et créer `logs\` avec droits d'écriture.

### Frontend (`frontend/`)
```bash
# pointer vers l'URL publique de l'API
set VITE_API_URL=https://api.example.com
npm run build
```
- `frontend/public/web.config` (règle SPA + MIME) est copié dans `dist/`.
- Copier `dist/` dans un second Site IIS.
- Activer HTTPS : le Web Speech API (micro du chat) ne fonctionne qu'en
  HTTPS hors localhost.

### CORS
Si front et back sont sur des origines différentes, ajuster la policy
CORS dans `backend/Program.cs` (`WithOrigins(...)`) puis republier.
