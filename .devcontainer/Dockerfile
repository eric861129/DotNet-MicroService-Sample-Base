FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm

# Keep the image small and focused. The devcontainer features layer
# adds Docker, Azure CLI and GitHub CLI on top of this base image.
RUN apt-get update \
    && apt-get install -y --no-install-recommends jq \
    && rm -rf /var/lib/apt/lists/*
