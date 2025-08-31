# BroPilot

BroPilot is a Visual Studio extension that brings the power of local Large Language Models (LLMs) directly into your development workflow. Unlike cloud-based AI assistants, BroPilot enables intelligent code assistance—such as code completions, explanations, and refactoring suggestions—without sending your code or context to external servers. This ensures privacy, low latency, and full control over your development environment.

---

## Problem Statement

Modern software development increasingly relies on AI-powered code assistants to boost productivity, improve code quality, and reduce repetitive tasks. However, most existing solutions require sending sensitive code to third-party cloud services, raising concerns about privacy, data security, and compliance. Additionally, reliance on external APIs can introduce latency, cost, and dependency risks.

**BroPilot addresses these challenges by:**
- Allowing developers to leverage advanced code assistance features powered by local LLMs.
- Ensuring that all code and context remain on the developer's machine.
- Providing a seamless, integrated experience within Visual Studio.

---

## Key Features

### 1. **Local LLM Integration**
- Connects to local LLM servers (such as Ollama, LM Studio, or other compatible backends).
- No code or context is sent to the cloud—everything stays on your machine.

### 2. **Code Explanations**
- Select code and receive clear, concise explanations powered by your local model.
- Useful for onboarding, code reviews, and understanding legacy code.

### 3. **Refactoring Assistance**
- Get suggestions for improving code structure, readability, and maintainability.
- Supports common refactoring patterns and best practices.

### 4. **Seamless Visual Studio Integration**
- Native Visual Studio extension—no need to switch tools or editors.

### 5. **Configurable and Extensible**
- Easily configure the extension to connect to your preferred local LLM backend.

### 6. **Privacy and Security**
- All processing is performed locally; no data leaves your machine.
- Ideal for organizations with strict compliance or confidentiality requirements.

---

## Getting Started

1. **Install BroPilot**  
   Download and install the BroPilot extension from the Visual Studio Marketplace or build from source.

2. **Set Up a Local LLM Server**  
   Start your preferred local LLM backend (e.g., Ollama, LM Studio) and ensure it is accessible.

3. **Configure BroPilot**  
   In Visual Studio, open the BroPilot settings and specify the connection details for your local LLM server.

4. **Start Coding**  
   Use BroPilot features directly from the editor—trigger completions, request explanations, or refactor code with a click.

---

## Requirements

- Visual Studio 2022 or later
- .NET Framework 4.8 and/or .NET 8 (for project compatibility)
- A running local LLM server (Ollama, LM Studio, or compatible)

---

## License

BroPilot is licensed under the [Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License](LICENSE).  
This means you are free to use and share the extension for non-commercial purposes, but you may not modify or use it for commercial gain.

---

## Acknowledgments

- Inspired by the growing ecosystem of local LLMs and privacy-first developer tools.
- Thanks to the open-source community for making local AI accessible to all.

---

## Contact

For support, questions, or feedback, please open an issue on the [GitHub repository](https://github.com/Ericvf/BroPilot).
