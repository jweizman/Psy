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
