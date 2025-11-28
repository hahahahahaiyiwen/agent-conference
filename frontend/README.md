# Agent Conference Frontend

A chatroom-style interface that lets you submit problems to the Agent Conference Web API and watch the discussion unfold in real time.

## Getting started

```pwsh
cd frontend
npm install
npm run dev
```

By default the UI expects the API to be served from the same origin. To point it at a different base URL set `VITE_API_BASE_URL` before running the dev server:

```pwsh
$env:VITE_API_BASE_URL = "https://localhost:5001"
npm run dev
```

## Behaviour

- **Problem panel (right):** enter a statement, optional context, and tuning options such as attendee count and time limit. Submission starts an asynchronous solve request.
- **Chatroom panel (left):** polls the monitoring endpoint for live room events until it receives `404`, at which point it fetches the final async operation result and displays it in the problem panel.
- Errors and operation identifiers are surfaced in the UI to aid troubleshooting.
