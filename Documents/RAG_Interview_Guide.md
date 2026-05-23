# RAG Pipeline & Semantic Search: Interview Guide

This document is designed to help you explain the core concepts of Retrieval-Augmented Generation (RAG) during software engineering and AI engineering interviews. It covers how text is processed, embedded, and retrieved in the DebugSense AI project.

---

## 1. The Chunking Strategy

**Interview Question:** *"Why didn't you just chunk the StackOverflow data by character limit (e.g., every 500 characters)?"*

**Answer:**
Naive character-based chunking is a common pitfall in basic RAG implementations. If we chunk purely by character limit, we risk slicing a critical C# stack trace or a block of code right down the middle, destroying its structural integrity. 

In this project, I implemented **Semantic HTML Chunking**. The parser iterates through the DOM tree:
- `<pre>` and `<code>` blocks are extracted completely intact as pure `Code` chunks.
- Everything else is stripped of HTML noise, normalized, and extracted as `Text` chunks.

This ensures that the embedding model receives dense, high-quality data without being polluted by HTML tags like `<div>` or `<em>`, which degrade semantic matching.

---

## 2. What is an Embedding (Vector)?

**Interview Question:** *"What exactly is an embedding, and why do we need it?"*

**Answer:**
An embedding is a representation of text as a dense vector (a large array of floating-point numbers) in a multi-dimensional space. For example, OpenAI's `text-embedding-3-small` outputs an array of 1,536 numbers for a given piece of text.

We need it because it captures the **semantic meaning** of the text, rather than just the literal words. For instance, the phrases "memory leak" and "OutOfMemoryException" don't share any keywords, but an embedding model understands they are conceptually related. Therefore, it will map both phrases to coordinates that are mathematically very close to each other in that 1,536-dimensional space.

---

## 3. Semantic Search vs. Keyword Search

**Interview Question:** *"Why use a Vector Database instead of just searching PostgreSQL with a SQL `LIKE` query or using Elasticsearch?"*

**Answer:**
Standard SQL `LIKE` queries or basic BM25 keyword search (Elasticsearch) look for exact word matches. If a user queries *"App crashes when database is locked"*, a keyword search will fail if the database only contains the phrase *"SqlException: Timeout expired"*.

Semantic search solves this using **Cosine Similarity**. 
When a user types a query, we:
1. Convert the query into a vector using the exact same embedding model.
2. Go to the Vector Database (like Qdrant) and ask it to calculate the mathematical distance (Cosine Similarity) between the user's vector and all the vectors in our database.
3. The database returns the closest vectors. Because embeddings capture *intent*, "database locked" and "timeout expired" will have a very high similarity score.

*(Note: In Phase 3 of this project, we actually combine both approaches into **Hybrid Search**, as Keyword Search is still very useful for exact Exception names.)*

---

## 4. The RAG Flow (Overcoming the LLM Context Window)

**Interview Question:** *"Why do we need RAG? Why not just put all the data in the LLM prompt?"*

**Answer:**
Large Language Models have a limited "Context Window" (memory limit) and charge per token. You cannot paste 10,000 StackOverflow questions into a prompt. Furthermore, fine-tuning an LLM on your specific data is extremely expensive, slow, and prone to hallucinations.

RAG (Retrieval-Augmented Generation) solves this by separating retrieval from generation:
1. **Retrieve:** We use our Vector Database to execute a semantic search and pull only the top 3-5 most relevant chunks (e.g., the exact code block and accepted answer).
2. **Augment:** We inject those 3-5 chunks directly into a prompt template.
3. **Generate:** We tell the LLM, *"You are a debugging assistant. Answer the user's question using ONLY the provided context."*

This guarantees the LLM answers using our verified, grounded data, massively reducing hallucinations and saving token costs.
