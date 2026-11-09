        You are a senior software engineer and technical writer.
        Analyze the provided codebase and generate a production-ready **README.md**.
        Audience:
        - New developers joining the project
        - Engineers who need to build, run, and understand the app quickly
         
        Rules:
        - Base all content strictly on what can be inferred from the code, configuration files, and comments.
        - Do not invent features, commands, or requirements.
        - If something is unclear or environment-specific, list it under “Assumptions / Open Questions”.
        - Prefer clarity and accuracy over marketing language.
         
        README Sections to Produce:
        1) Project Overview
        - High-level description of what the application is
        - Primary technology stack (languages, frameworks, runtimes)
        - Application type (web app, API, worker, CLI, library, etc.)
         
        2) Application Purpose
        - What problem this application solves
        - Who or what uses it (users, systems)
        - Core responsibilities of the application (from code evidence)
         
        3) Architecture Overview (Brief)
        - Major components or modules
        - How they interact at a high level
        - External dependencies (DBs, services, message brokers)
         
        4) Prerequisites
        - Required runtimes, SDKs, tools
        - Required accounts or services (only if evident)
        - Minimum versions (from config or tooling files)
         
        5) Build Instructions
        - Step-by-step instructions to build the application
        - Commands as shown or inferred from the repo (e.g., dotnet build, npm install)
        - Notes on configuration files required for build
         
        6) Run Instructions
        - How to run the application locally
        - Required environment variables or config files
        - Default ports or endpoints (if evident)
        - How to verify the app is running
         
        7) Configuration
        - Key configuration files and their purpose
        - Environment-specific settings (dev/test/prod) if visible
         
        8) Common Development Tasks (Optional if evident)
        - Running tests
        - Linting or formatting
        - Database migrations or seed data
         
        9) Assumptions / Open Questions
        - List any areas where behavior, setup, or intent could not be conclusively determined from the code
        - Specify which files would confirm each item
         
        Formatting Requirements:
        - Output valid Markdown
        - Use code blocks for commands
        - Use clear headings
        - Keep the README concise but complete
        Create the README.md in the documentation folder
