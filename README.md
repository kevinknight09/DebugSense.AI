# DebugSense AI: StackOverflow RAG for Debugging

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)

DebugSense AI is an AI-powered debugging assistant that uses Retrieval-Augmented Generation (RAG) to help developers diagnose software issues. By retrieving relevant debugging discussions, documentation, and solutions from a curated StackOverflow dataset, it generates highly grounded and context-aware answers to exception messages, stack traces, and error logs.

## 🚀 Core Features (In Progress)
- **Semantic Search**: Understands the meaning of error messages, not just keyword matches.
- **Hybrid Retrieval**: Combines BM25 keyword search with Vector similarity to find precise answers.
- **Local AI Orchestration**: Uses Ollama for running embedding models and LLMs locally.
- **Grounded Responses**: Provides source citations from StackOverflow to reduce AI hallucinations.

## 🏗️ Architecture
1. **Data Ingestion**: Fetches top C# exception questions from the StackExchange API.
2. **Chunking & Parsing**: Intelligently splits problem descriptions, stack traces, and accepted answers into chunks.
3. **Embeddings**: Converts text chunks into vector representations using local Ollama models.
4. **Vector Database**: Stores embeddings and metadata (framework, exception type) in Qdrant for fast semantic search.

## 🛠️ Technology Stack
- **Backend Core**: ASP.NET Core / C#
- **Embeddings & LLM**: Ollama (Local)
- **Vector Database**: Qdrant
- **Deployment**: Docker Compose

## 📁 Project Structure
```text
/src
  /DataFetcher        # Fetches StackOverflow data via StackExchange API
  /Parsers            # Splits documents and logs into intelligent chunks
  /Embedding          # Interfaces with Ollama for generating vector embeddings
  /Infrastructure     # Integrates with Qdrant Vector DB
  /Retrieval          # (Upcoming) Executes hybrid queries and reranking
  /Api                # (Upcoming) Exposes backend endpoints for frontend
```

## ⚙️ Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop) and Docker Compose

### 1. Start Infrastructure Services
The project uses Docker to host Qdrant (Vector DB) and Ollama (Local AI). Start these services in the background:

```powershell
cd docker
docker compose up -d
```

*Note: Qdrant will run on ports `6333` and `6334`, and Ollama will run on port `11434`.*

### 2. Fetch Sample Dataset
Run the data fetcher to download a sample of top C# exception questions from StackOverflow:

```powershell
cd ../src/DataFetcher
dotnet run
```
This will create a `sample_dataset.json` file in the `data/` directory.

## 🗺️ Roadmap
- **Phase 1 (MVP)**: Implement end-to-end ingestion, basic vector retrieval, and simple LLM generation.
- **Phase 2**: Add Hybrid Search (BM25 + Vector) and metadata filtering.
- **Phase 3**: Introduce query rewriting, error fingerprinting, and framework-aware context.
- **Phase 4**: Develop a rich frontend (Angular) with streaming responses (SignalR).

## 🛡️ License
This project is open-source. Please see the LICENSE file for details.
