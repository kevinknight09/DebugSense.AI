# The Retrieval Process (RAG Phase 2)

This document outlines the architecture and workflow of the **Retrieval Layer** within the DebugSense application. The Retrieval Layer is the core engine that bridges the gap between a user's natural language error query and the vast knowledge base stored in our vector database.

---

## 1. High-Level Overview

In a Retrieval-Augmented Generation (RAG) system, **Retrieval** is the process of finding the most mathematically relevant pieces of context (documents, code snippets, etc.) from a database based on a user's prompt. 

Instead of relying on traditional keyword matching (like `SQL LIKE %query%`), our system uses **Semantic Search**. This allows the system to understand the *meaning* behind an error (e.g., knowing that "Null Reference" and "Object not set to an instance" mean the same thing).

---

## 2. Core Components

The Retrieval Layer consists of three primary C# services working in harmony:

1. **`SemanticSearchService`** (The Orchestrator)
   Located in the `DebugSense.Retrieval` project, this service manages the overall search workflow. It takes a raw string, coordinates with the embedding model, queries the database, and returns clean C# Data Transfer Objects (`SearchResult`).

2. **`OllamaEmbeddingService`** (The Translator)
   Located in the `DebugSense.Embedding` project, this service sends HTTP requests to our local Ollama instance running the `nomic-embed-text` model. It translates human text into a 768-dimensional mathematical vector.

3. **`QdrantVectorStore`** (The Search Engine)
   Located in the `DebugSense.Infrastructure` project, this service connects to our local Qdrant container. It performs high-speed **Cosine Similarity** searches using the HNSW (Hierarchical Navigable Small World) algorithm.

---

## 3. The Step-by-Step Workflow

When a user types a query (e.g., *"How do I fix a NullReferenceException?"*) into the Playground or future Web API, the following pipeline executes:

### Step 1: Prefix Application
The `OllamaEmbeddingService` intercepts the user's query. Because we are using the `nomic-embed-text` model, it requires specific instructions to perform optimally. The service automatically prepends the query with the text `"search_query: "`.
*(Note: During the data ingestion phase, documents were prefixed with `"search_document: "`)*.

### Step 2: Vectorization
The prefixed text is sent to Ollama on port `11434`. The neural network analyzes the semantics of the text and returns an array of precisely **768 floating-point numbers** (the vector). 

### Step 3: Vector Search
The 768-dimensional vector is passed to the `QdrantVectorStore`. Qdrant searches the `stackoverflow_chunks` collection. It calculates the Cosine Similarity (the mathematical angle) between the user's vector and every vector stored in the database.

### Step 4: Result Extraction
Qdrant returns the top `N` closest vectors (default is 3). It also returns the `Payload` attached to those vectors, which contains the raw text, the original StackOverflow Title, and the Chunk Type (Code vs. Text).

### Step 5: Formatting
The `SemanticSearchService` takes these raw Qdrant payloads and maps them into clean `SearchResult` objects containing the `Score` and the `Content`, returning them to the user.

---

## 4. Performance & Precision Tuning

During the development of this layer, we implemented several optimizations to ensure high-precision results:

* **Instruction Prefixes**: By explicitly defining `search_query: ` and `search_document: `, the Nomic model's accuracy increases significantly, preventing the model from confusing a question with an answer.
* **Batching**: Database insertions were batched in groups of 50 to prevent memory overflow and API timeouts during the initial data load.
* **Vector Dimensions**: Qdrant was explicitly configured to require exactly 768 dimensions, ensuring structural integrity between the LLM output and the database schema.
