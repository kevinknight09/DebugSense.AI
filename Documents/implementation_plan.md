# DebugSense AI: Project Structure & Implementation Plan

This document outlines the proposed folder structure and a detailed, step-by-step roadmap for building your StackOverflow RAG Debugging Assistant (DebugSense AI). Since this is a resume project, the plan focuses heavily on highlighting engineering best practices, architectural decisions, and measurable outcomes.

## Proposed Project Structure

We will create the following directory structure in your workspace (`e:\Projects\StackoverFlowRag`):

```text
e:\Projects\StackoverFlowRag
├── src/
│   ├── Api/             # ASP.NET Core Web API (Controllers, SignalR hubs, DI setup)
│   ├── Application/     # Business logic, use cases, and interfaces (Core logic)
│   ├── Infrastructure/  # External integrations (Qdrant, DBs, OpenAI/Ollama clients)
│   ├── Retrieval/       # Search logic (Vector search, BM25, Hybrid search orchestration)
│   ├── Embedding/       # Text chunking logic and embedding generation pipelines
│   ├── Reranking/       # Cross-encoder or LLM-based reranking services
│   ├── Parsers/         # Custom C# services for parsing stack traces and unstructured logs
│   └── Frontend/        # Angular frontend application
├── docker/              # Docker Compose configurations (Qdrant, Elasticsearch, Redis, etc.)
├── data/                # Sample datasets, raw exports, and evaluation sets
└── docs/                # Architecture diagrams, API specs, and evaluation metrics
```

## Detailed Step-by-Step Roadmap (Resume Focused)

Building this project iteratively is the best way to ensure quality and understand every moving part. Here is how we will proceed step-by-step, along with the specific engineering skills each step demonstrates for your resume.

### Step 1: Data Pipeline & Embedding Foundation (Focus: Data Engineering & Vector DBs)
**What we will do:**
- Gather a small dataset (5k-10k items) of StackOverflow issues or GitHub bug reports.
- Write custom parsers in C# (`src/Parsers`) to intelligently chunk this data (separating problem descriptions, code blocks, and accepted answers, rather than blind character-count chunking).
- Use an embedding model (like `text-embedding-3-small` or a local BGE model) to convert these chunks into vectors (`src/Embedding`).
- Store the vectors along with metadata (framework, exception type) in Qdrant (`src/Infrastructure`).

**Resume Impact:** *Demonstrates your ability to design semantic data pipelines, perform intelligent text chunking, and work with modern Vector Databases (Qdrant).*

### Step 2: Core Backend & Naive RAG (Focus: Backend Engineering & LLM Integration)
**What we will do:**
- Scaffold the ASP.NET Core API (`src/Api`) and the Application layer (`src/Application`).
- Implement basic semantic vector search: take a user's error, embed it, and find the nearest neighbors in Qdrant.
- Build a context window and inject the retrieved documents into an LLM prompt.
- Return the generated response.

**Resume Impact:** *Showcases your ability to build a standard Retrieval-Augmented Generation (RAG) backend using enterprise frameworks like ASP.NET Core.*

### Step 3: Hybrid Search (Focus: Search Relevance Optimization)
**What we will do:**
- Implement BM25 Keyword Search (using Elasticsearch or LiteDB) in the `src/Retrieval` layer.
- Combine the results of the Vector Search (good for semantic meaning) and BM25 Search (good for exact exception names and method names).
- Implement Reciprocal Rank Fusion (RRF) to merge the two score lists.

**Resume Impact:** *A huge differentiator for a resume. "Engineered a hybrid retrieval system combining BM25 keyword search and vector similarity, significantly improving technical issue matching."*

### Step 4: Reranking & Hallucination Mitigation (Focus: Advanced AI Engineering)
**What we will do:**
- Pass the top results from the Hybrid Search through a Cross-Encoder Reranker (`src/Reranking`).
- Implement strict confidence thresholds and prompt constraints to force the LLM to admit when it doesn't know the answer, rather than hallucinating fake fixes or APIs.

**Resume Impact:** *Proves you understand that standard RAG isn't enough for production. "Implemented cross-encoder reranking and strict citation grounding to drastically improve retrieval precision and minimize LLM hallucinations."*

### Step 5: Frontend & Real-Time Streaming (Focus: Full-Stack & UX)
**What we will do:**
- Build the Angular UI (`src/Frontend`) with an interface that accepts stack traces and displays retrieved sources alongside the AI answer.
- Connect the frontend to the ASP.NET Core backend using SignalR to stream the LLM response token-by-token (ChatGPT-style UX).

**Resume Impact:** *Demonstrates full-stack capabilities and modern real-time web communication. "Developed an Angular SPA with real-time, token-by-token streaming responses using ASP.NET Core SignalR."*

### Step 6: Evaluation & Dockerization (Focus: MLOps & DevOps)
**What we will do:**
- Write scripts to formally evaluate your system using metrics like **Retrieval Precision** and **Recall@K**.
- Create a `docker-compose.yml` to spin up the API, Qdrant, Elasticsearch, and the Frontend simultaneously.

**Resume Impact:** *Employers love candidates who measure their AI systems. "Established quantitative evaluation metrics for RAG accuracy and Dockerized the complete microservice architecture for seamless deployment."*

## User Review Required

Please review the proposed folder structure and the step-by-step roadmap. 

> [!IMPORTANT]
> If you approve, I will proceed to create this physical folder structure on your disk (`e:\Projects\StackoverFlowRag`), initialize the base ASP.NET Core and Angular projects within them, and set up our `task.md` to begin Step 1.

Are you ready to proceed with creating the project structure?
