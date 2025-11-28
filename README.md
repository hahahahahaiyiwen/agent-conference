# Agent Conference

LLM*s*-as-Judge

## Introduction

Introducing AgentConference, a system that leverages general-purpose models with minimal prompt engineering to orchestrate debates and form consensus, creating a "meta-agent" evaluator that's dynamic, collaborative, and less prone to the biases of a single LLM judge.

## Quick Start

### Prerequisites

- .NET 8 SDK (for the API)
- Node.js 18+ and npm (for the frontend)

### 1. Configure the backend

The API expects its settings in `src/AgentConference.WebApi/appsettings.json`. Update these values for your environment:

```json
"AzureAIService": {
  "Endpoint": "https://<your-azure-openai-endpoint>/",
  "ApiKey": "<your-api-key>",
  "DeploymentNames": [
    "<your-model-deployment-name-1>",
    "<your-model-deployment-name-2>"
  ]
}
```

Identity based credential will be used if no API Key is provided. Please make sure run following commands before using:
az login

Then restore and run the Web API:

```pwsh
cd src/AgentConference.WebApi
dotnet restore
dotnet run
```

By default the API listens on `https://localhost:7132` and `http://localhost:5132`. Note the URL you plan to expose to the frontend.

### 2. Configure the frontend

From the repo root:

```pwsh
cd frontend
npm install
```

Set the API base URL environment variable before starting the dev server:

```pwsh
setx VITE_API_BASE_URL "https://localhost:7132"
# restart the terminal so the new value is loaded
```

For macOS/Linux shells:

```bash
export VITE_API_BASE_URL=https://localhost:7132
```

Start the Vite dev server:

```pwsh
npm run dev
```

Vite serves the UI at `http://localhost:5173` by default. Open it in a browser; the UI proxies calls to the API using the configured `VITE_API_BASE_URL`.

### 3. Optional: serve frontend from the API

To host everything from one origin, build the frontend and copy the output into the API's static files folder (for example `wwwroot`). Then update `Program.cs` to serve static files and map the SPA fallback. In that setup you can omit the `VITE_API_BASE_URL` because requests come from the same origin.

### Troubleshooting

- **CORS errors:** ensure the API has CORS enabled for the frontend origin when running separately.
- **Missing Azure settings:** double-check the endpoint and deployment IDs in `appsettings.json`.
- **Frontend cannot reach API:** confirm `VITE_API_BASE_URL` matches the API URL (schema + host + port).
