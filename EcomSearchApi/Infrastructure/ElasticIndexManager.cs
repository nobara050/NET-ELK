using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using EcomSearchApi.Models;

namespace EcomSearchApi.Infrastructure;

// Manages Elasticsearch index lifecycle, settings, analyzers, and type mappings
public class ElasticIndexManager(
    ElasticsearchClient client, 
    IConfiguration configuration, 
    ILogger<ElasticIndexManager> logger)
{
    private readonly string _indexName = configuration["Elasticsearch:IndexName"] ?? "products";

    // Recreates index with Edge N-Gram custom analyzer and field mappings
    public async Task RecreateIndexAsync()
    {
        var existsResponse = await client.Indices.ExistsAsync(_indexName);
        if (existsResponse.Exists)
        {
            logger.LogInformation("Deleting existing Elasticsearch index {IndexName}...", _indexName);
            var deleteResponse = await client.Indices.DeleteAsync(_indexName);
            if (!deleteResponse.IsValidResponse)
            {
                throw new InvalidOperationException($"Failed to delete index: {deleteResponse.DebugInformation}");
            }
        }

        logger.LogInformation("Creating Elasticsearch index {IndexName} with Edge N-Gram analyzer & mappings...", _indexName);

        var createResponse = await client.Indices.CreateAsync(_indexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .TokenFilters(tf => tf
                        .EdgeNGram("autocomplete_filter", eng => eng
                            .MinGram(2)
                            .MaxGram(10)
                        )
                    )
                    .Analyzers(anz => anz
                        .Custom("autocomplete_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter(["lowercase", "autocomplete_filter"])
                        )
                    )
                )
            )
            .Mappings(m => m
                .Properties<Product>(p => p
                    .Text(t => t.Name, tt => tt
                        .Fields(f => f
                            .Keyword("keyword")
                            .Text("autocomplete", a => a
                                .Analyzer("autocomplete_analyzer")
                                .SearchAnalyzer("standard")
                            )
                        )
                    )
                    .Text(t => t.Description, tt => tt.Analyzer("english"))
                    .Keyword(t => t.Category)
                    .Keyword(t => t.Brand)
                    .ScaledFloatNumber(t => t.Price, sf => sf.ScalingFactor(100))
                    .IntegerNumber(t => t.Stock)
                    .Keyword(t => t.Tags)
                    .Date(t => t.CreatedAt)
                )
            )
        );

        if (!createResponse.IsValidResponse)
        {
            logger.LogError("Create index failed: {Debug}", createResponse.DebugInformation);
            throw new InvalidOperationException($"Create index failed: {createResponse.DebugInformation}");
        }

        logger.LogInformation("Elasticsearch index {IndexName} created successfully.", _indexName);
    }
}
