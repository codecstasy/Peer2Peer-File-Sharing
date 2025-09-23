# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Common Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run in development mode with Swagger UI
dotnet run

# Run with specific profile
dotnet run --launch-profile https

# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore
```

### Testing and Development
```bash
# Run the application (opens Swagger at https://localhost:7142 or http://localhost:5024)
dotnet run

# Watch mode for development (auto-reload on file changes)
dotnet watch run

# Build for release
dotnet build -c Release

# Publish application
dotnet publish -c Release -o ./publish
```

### Project Structure Commands
```bash
# Add new NuGet package
dotnet add package PackageName

# Create new controller
dotnet aspnet-codegenerator controller -name NewController -api

# View project references
dotnet list reference
```

## Architecture Overview

This is a **Peer-to-Peer File Sharing** system built with **ASP.NET Core 8.0** and designed around a distributed architecture where files are chunked for efficient sharing across peers.

### High-Level Architecture

**Core Components:**
- **File Processing Pipeline**: FileChunker → DistributedMetadata → FileAssembler
- **Peer Network Management**: Peer discovery, heartbeat, and communication protocols
- **Chunk-based Distribution**: Files split into 1MB chunks with SHA256 integrity verification
- **RESTful API**: HTTP endpoints for file operations and peer communication

### Key Architectural Patterns

**Service Layer Pattern:**
- `IFileChunker` / `FileChunker`: Splits files into chunks with metadata
- `IFileAssembler` / `FileAssembler`: Reconstructs files from chunks
- `IPeerDiscoveryService`: Manages peer registration and discovery (interface only)

**Domain Models:**
- `Peer`: Full peer representation with gossip protocol support
- `PeerInfo`: Lightweight peer data for network sharing  
- `DistributedMetadata`: Enhanced metadata tracking chunk-to-peer mapping
- `Metadata`: Basic file metadata with chunk hashes for integrity

**Communication Models:**
- Request/Response patterns for peer registration, heartbeat, chunk discovery
- Chunk announce/discovery for distributed file location
- Network statistics monitoring

### File Distribution Flow

1. **Upload**: File → FileChunker → Multiple chunks + Metadata
2. **Distribution**: Chunks distributed across peers with tracking in DistributedMetadata
3. **Discovery**: Peers discover which peers have required chunks
4. **Download**: Request chunks from multiple peers simultaneously  
5. **Assembly**: FileAssembler reconstructs original file with integrity verification

### Network Communication

**Peer Discovery:**
- Gossip protocol implementation for peer discovery
- Heartbeat mechanism for peer liveness tracking
- Random peer selection for network resilience

**Chunk Management:**
- Rarest-first chunk prioritization for optimal distribution
- Chunk-to-peer mapping for efficient location
- Replication priority system for important files

### Development Notes

**Logging**: Comprehensive Serilog integration with multiple output levels (Debug, Info, Warning, Error) saved to rotating daily logs in `logs/` directory.

**API Endpoints:**
- `POST /api/file/upload`: Upload and chunk files
- `POST /api/file/assemble`: Reconstruct files from chunks
- Swagger UI available at `/swagger` in development

**Configuration**: 
- Application runs on ports 5024 (HTTP) and 7142 (HTTPS)
- Chunk size: 1MB default
- Peer limit: 50 known peers maximum per node
- Log retention: 7 days

**File Structure:**
- Chunks stored in temporary directories under system temp path
- Metadata files saved as JSON alongside chunks
- Hash verification using SHA256 for chunk integrity