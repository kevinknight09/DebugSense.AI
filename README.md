# DebugSense AI: 100% Private Offline RAG System, Made using .NET | Ollama | Angular

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Angular](https://img.shields.io/badge/angular-%23DD0031.svg?style=for-the-badge&logo=angular&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![Ollama](https://img.shields.io/badge/Ollama-Local_LLM-black?style=for-the-badge&logo=ollama)

DebugSense AI is an AI-powered debugging assistant that uses Retrieval-Augmented Generation (RAG) to help developers diagnose software issues. By retrieving relevant debugging discussions, documentation, and solutions from a curated StackOverflow dataset, it generates highly grounded and context-aware answers to exception messages, stack traces, and error logs.

<p align="center">
  <video src="./assets/Final_Demo.mp4" width="100%" controls></video>
  <br>
  <em>*Note: This demonstration video has been sped up for presentation purposes. The original implementation has longer model-load times depending on hardware.*</em>
</p>

  <img src="./assets/UI_Demo_AfterResponse.png" width="100%" alt="UI After Response" />

## 🚀 Core Features
- **Data Ingestion Pipeline**: Automatically downloads and cleans C# StackOverflow threads.
- **Smart Chunking**: Separates conversational text from HTML `<pre>` code blocks for precise embedding.
- **Semantic Search**: Understands the meaning of error messages using the `nomic-embed-text` model.
- **Local AI Orchestration**: Uses Ollama with `phi3` or `llama3` for running entirely offline, private, and free LLM generations.
- **Hardware Optimized**: Supports direct GPU passthrough to Docker containers for blazing-fast inference on Windows.
- **Interactive Playground**: A built-in terminal CLI to chat with your codebase errors.

## 🏗️ Architecture
1. **Data Ingestion**: `DataFetcher` pulls data, `DocumentParser` cleans it, and `DataIngestor` orchestrates the pipeline.
2. **Embeddings**: Converts text chunks into 768-dimensional mathematical vectors. Applies `search_document:` and `search_query:` prefixes for maximum Nomic model accuracy.
3. **Vector Database**: Stores embeddings in a local Qdrant container for high-speed HNSW Cosine Similarity search.
4. **Generation (RAG)**: The `RagOrchestratorService` extracts the top K chunks and prompts `phi3` to synthesize a tutoring-style answer.
5. **Presentation (UI)**: A modern Angular SPA frontend connects to the C# API and streams the LLM response in real-time via Server-Sent Events (SSE).

```mermaid
graph TD
    UI[Angular Web UI] -->|SSE Stream| API(ASP.NET Core API)
    API -->|Query| RAG{RagOrchestrator}
    RAG -->|1. Vectorize Query| Embed[Ollama: nomic-embed-text]
    Embed -->|768d Vector| RAG
    RAG -->|2. Search| Qdrant[(Qdrant Vector DB)]
    Qdrant -->|Top 3 Chunks| RAG
    RAG -->|3. Prompt + Context| LLM[Ollama: phi3]
    LLM -->|Streamed Tokens| RAG
    RAG -->|Stream| API
```

## 🛠️ Technology Stack
- **Frontend UI**: Angular (TypeScript)
- **Backend API**: ASP.NET Core Web API (Server-Sent Events)
- **Backend Core**: .NET 10.0 / C#
- **Embeddings & LLM**: Ollama (Local)
- **Vector Database**: Qdrant
- **Deployment**: Docker Compose

## 📁 Project Structure
```text
/src
  /DataFetcher        # Fetches StackOverflow data via StackExchange API
  /Parsers            # Splits documents and logs into intelligent chunks
  /Application        # Clean Architecture Core (Interfaces, DTOs, Use Cases)
  /Embedding          # Interfaces with Ollama for generating vector embeddings
  /Infrastructure     # Integrates with Qdrant Vector DB & Ollama Chat
  /Retrieval          # Orchestrates hybrid queries and RAG generation
  /DataIngestor       # Console app that builds the vector database
  /Playground         # Interactive CLI to test the RAG engine
  /Api                # ASP.NET Core Web API with Server-Sent Events (SSE)
  /UI                 # Dark-mode Angular SPA web frontend
```

## ⚙️ Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Node.js & npm](https://nodejs.org/) (Required for the Angular UI)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- *Optional but highly recommended: NVIDIA GPU for fast AI generation.*

### 1. Start Infrastructure Services
The project uses Docker to host Qdrant (Vector DB) and Ollama (Local AI). Start these services in the background:

```powershell
cd docker
docker compose up -d
```
*Note: If you have an NVIDIA GPU, the docker-compose file is already pre-configured to pass your GPU directly into Ollama for maximum speed.*

### 2. Pull Required AI Models
You will need to pull the specific local models we use into Ollama:
```powershell
docker compose exec ollama ollama pull nomic-embed-text
docker compose exec ollama ollama pull phi3
```

### 3. Fetch and Ingest Data
Run the fetcher to download the dataset, and the ingestor to embed it into the database:
```powershell
cd ../src/DataFetcher
dotnet run

cd ../DataIngestor
dotnet run
```
*This will vectorize the StackOverflow dataset and upload it to Qdrant.*

### 4. Talk to the AI
Run the Playground to open an interactive chat terminal:
```powershell
cd ../Playground
dotnet run
```
Type your exception (e.g. `Null reference object not set to an instance`), and the AI will scan the vector database and generate an expert response!

### 5. Launch the Enterprise Web Application
To experience the RAG engine in a sleek, dark-mode browser interface with live streaming:
1. Open a terminal and start the ASP.NET Core API:
```powershell
cd src/Api
dotnet run
```
2. Open a second terminal and start the Angular frontend:
```powershell
cd src/UI
npm start
```
Open `http://localhost:4200` in your browser to start chatting!

## 🗺️ Roadmap
- ✅ **Phase 1**: Implement end-to-end ingestion and infrastructure.
- ✅ **Phase 2**: Vector similarity search (Retrieval Layer).
- ✅ **Phase 3**: RAG Orchestrator and Local LLM Generation.
- ✅ **Phase 4**: Developed a rich web frontend (Angular) and API with live SSE streaming.
- ⏳ **Phase 5**: Add Hybrid Search (BM25 + Vector) and metadata filtering.
- ⏳ **Phase 6**: Conversational Memory (PostgreSQL Chat History integration).
- ⏳ **Phase 7**: Package the Angular application as an offline Desktop App using Electron.



