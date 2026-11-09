		You are a senior systems engineer and software architect. Analyze the provided codebase and produce a “Data Flow & I/O Specification” document.
		 
		Goal: determine how data enters the system, how it moves/transforms through components, where it is stored, and how it leaves the system.
		 
		Rules:
		- Use only evidence from the code I provide. If something is uncertain, mark it as “Assumption” and list exactly what file/function would confirm it.
		- Prefer concrete identifiers: file paths, class names, function names, endpoints, queues, table names, topics, event types, DTOs.
		- Do not summarize “business intent” unless it is explicitly stated in code/comments.
		- If multiple flows exist, separate them by feature/use-case or by entrypoint.
		 
		Deliverables (in this order):
		1) System Context (1 page max)
		- What kind of app(s) this is (web/API/worker/CLI), main runtime(s), and top-level modules.
		 
		2) Data Entry Points (Inputs)
		For each entry point, include:
		- Type: HTTP endpoint / message queue / scheduled job / file ingest / CLI / UI / webhook / DB trigger (etc.)
		- Location in code: path + symbol
		- Authentication/authorization checks (where enforced)
		- Validation/parsing steps
		- Primary input schema (fields, types) or best approximation from DTOs
		 
		3) Data Stores & External Dependencies
		- Databases (tables/collections), caches, file storage, third-party APIs
		- For each: what data is written/read, where in code, and key models used
		 
		4) Core Data Flows (the main section)
		For each flow, provide:
		- Flow name
		- Trigger/entry point
		- Step-by-step pipeline: component → transformation → output
		- Data transformations: mapping, enrichment, filtering, aggregation
		- Error paths: retries, dead-lettering, error responses, compensations
		- Observability: logs/metrics/traces emitted (where in code)
		 
		5) Outputs (System Exits)
		For each output, include:
		- Type: HTTP response / event published / DB write / email / file created / UI render (etc.)
		- Location in code
		- Output schema and where it’s constructed
		- Side effects and idempotency notes
		 
		6) Cross-Cutting Concerns
		- Transaction boundaries, consistency model, eventual consistency
		- Concurrency model (async, threading, worker pools)
		- Security and sensitive data handling (PII/PHI/etc.)—only if evident in code
		- Performance hotspots suggested by architecture (callouts only if evidence exists)
		 
		7) Diagrams
		- Mermaid diagram of data flow (graph LR or sequence diagram)
		- If multiple flows: one high-level diagram + one per major flow
		 
		8) “Evidence Index”
		- A bullet list mapping each claim to: file path + function/class name (and line ranges if available)
		 
Then proceed with the analysis, create a file in documentation folder, DataFlow.md