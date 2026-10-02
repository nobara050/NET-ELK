# E-Commerce Search & Observability System

A comprehensive reference implementation combining ASP.NET Core with the Elastic Stack (Elasticsearch, Logstash, Kibana) and PostgreSQL for high-performance product search, centralized log management, real-time metrics dashboards, and automated Discord alerting.

---

## Tech Stack

- **Backend**: ASP.NET Core (.NET 10), Entity Framework Core, Serilog
- **Primary Database**: PostgreSQL 16 (Single Source of Truth, ACID compliance)
- **Search Engine**: Elasticsearch 8.17 (Inverted index, BM25 relevance scoring, Edge N-Gram autocomplete, aggregations)
- **Log Processing**: Logstash 8.17 (TCP socket stream ingestion, timestamp parsing, log level normalization)
- **Observability & Analytics**: Kibana 8.17 (Discover, Kibana Lens dashboards, Stack Monitoring, Webhook alerting)
- **Notification Channel**: Discord Webhook (Automated alert dispatch)
- **Containerization**: Docker, Docker Compose

---

## Architecture

```text
[Client / Swagger]
       |
       v
[ASP.NET Core Web API]
   |                 \
   | (Dual-Write/     \ (TCP Socket :5000)
   |  Search Query)    \
   v                    v
[PostgreSQL :5432]   [Logstash :5000]
   &                    |
[Elasticsearch :9200] <-+ (Bulk Indexing: app-logs-*)
   ^
   |
[Kibana :5601] --------> [Discord Channel (Webhook POST)]
   (Lens Dashboards & Alert Rules)
```

---

## Core Capabilities

### 1. Advanced E-Commerce Search

- **Full-Text BM25 Search**: Multi-field search with field boosting (`name^2`) and search result highlighting.
- **Search-As-You-Type Autocomplete**: Custom Edge N-Gram token filter enabling prefix suggestion starting from 2 characters.
- **Fuzzy Search**: Levenshtein distance typo-tolerant matching for misspelled queries.
- **Compound Bool Queries**: Logical combination of `must`, `filter` (bitset caching), `should` (relevance boost), and `must_not`.
- **Aggregations**: Real-time statistical metrics calculation (min/max/average price) and categorical facets grouping.

### 2. Centralized Observability & Metrics

- **Structured Log Ingestion**: Serilog formats JSON events and transmits them over TCP directly to Logstash.
- **Interactive Dashboards**: Kibana Lens visualizations tracking log volume by severity, API response times (ms), HTTP status code distribution, and top requested endpoints.
- **Infrastructure Monitoring**: Native CPU, JVM Heap memory, and disk usage tracking via Elasticsearch self-monitoring.
- **Trace Correlation**: Request ID propagation across application layers for root-cause analysis.

### 3. Automated Alerting

- Real-time rule evaluation in Kibana scanning the `app-logs-*` index pattern.
- Automated payload construction using Discord Embed schema.
- Instant incident notifications delivered to Discord on error thresholds.

---

## Project Structure

```text
.
├── docker-compose.yml              # Multi-container definition (Elasticsearch, Logstash, Kibana)
├── logstash/
│   └── pipeline/
│       └── logstash.conf           # Logstash TCP input, filter, and Elasticsearch output pipeline
├── EcomSearchApi/                  # ASP.NET Core Web API project
│   ├── Controllers/                # Products, Search, Diagnostics, Reset controllers
│   ├── Services/                   # Product, Search, Diagnostics, Reset services
│   ├── Repositories/               # Data access repositories
│   ├── Infrastructure/             # Elasticsearch index configuration and analyzers
│   ├── Models/                     # Data models and seed datasets
│   ├── Data/                       # Entity Framework Core DbContext
│   ├── appsettings.json            # Serilog sinks and connection strings
│   └── Program.cs                  # Dependency injection and middleware pipeline
└── docs/
    ├── demo-elasticsearch.md       # Kibana Dev Tools Console API commands reference
    ├── demo-alert.md               # End-to-end alert trigger and verification demo script
    ├── demo-dashboard.md           # Step-by-step Kibana Lens dashboard construction guide
    ├── demo-kql.md                 # Complete Kibana Query Language (KQL) cheatsheet
    └── demo-logstash.md            # Logstash configuration syntax and plugin references
```

---

## Getting Started

### Prerequisites

- Docker Desktop
- .NET 10 SDK
- PostgreSQL instance (or configured Docker service)

### 1. Start Infrastructure

Run the following command from the project root directory:

```bash
docker compose up -d
```

Verify that `elasticsearch`, `logstash`, and `kibana` containers are running:

```bash
docker compose ps
```

### 2. Run the Web API

Navigate to the API directory and start the application:

```bash
cd EcomSearchApi
dotnet run
```

### 3. Access Service Interfaces

- **Swagger UI**: `http://localhost:5242`
- **Kibana Web UI**: `http://localhost:5601`
- **Elasticsearch API**: `http://localhost:9200`
- **Logstash TCP Input**: `localhost:5000`

### 4. Initialize Sample Data

Execute a POST request to seed both PostgreSQL and Elasticsearch simultaneously:

```bash
curl -X POST http://localhost:5242/api/Reset/reseed
```

---

## Documentation

For detailed step-by-step guides and reference commands, refer to:

- `demo-elasticsearch.md`: Dev Tools Console command reference (Cluster, Mapping, Query DSL, Aggregations).
- `demo-alert.md`: Step-by-step guide for testing Kibana Alerting with Discord Webhooks.
- `demo-dashboard.md`: Guide for creating visualizations and dashboards in Kibana Lens.
- `demo-kql.md`: Guide and cheatsheet for querying logs using Kibana Query Language.
- `demo-logstash.md`: Reference documentation for Logstash inputs, filters, and outputs.
