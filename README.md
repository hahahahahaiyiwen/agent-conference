# Agent Conference

LLM*s*-as-Judge

## Introduction

Introducing agent-conference, a multi-agent system that leverages general-purpose models with minimal prompt engineering to orchestrate debates and form consensus, creating a "meta-agent" evaluator that's dynamic, collaborative, and less prone to the biases of a single LLM judge.

<img width="1202" height="643" alt="image" src="https://github.com/user-attachments/assets/ba99ac91-ef25-4b63-9302-5a7d4ccc9d5c" />


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

By default the API listens on `https://localhost:7129` and `http://localhost:5141`. Note the URL you plan to expose to the frontend.

### 2. Configure the frontend

From the repo root:

```pwsh
cd frontend
npm install
```

Set the API base URL environment variable before starting the dev server:

```pwsh
setx VITE_API_BASE_URL "http://localhost:5141"
# restart the terminal so the new value is loaded
```

For macOS/Linux shells:

```bash
export VITE_API_BASE_URL=http://localhost:5141
```

Start the Vite dev server:

```pwsh
npm run dev
```

Vite serves the UI at `http://localhost:5173` by default. Open it in a browser; the UI proxies calls to the API using the configured `VITE_API_BASE_URL`.

