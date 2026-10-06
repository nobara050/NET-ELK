using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using EcomSearchApi.Models;

namespace EcomSearchApi.Repositories;

// Repository implementation for Elasticsearch operations
public class ProductSearchRepository(
    ElasticsearchClient es, 
    IConfiguration config, 
    ILogger<ProductSearchRepository> logger) : IProductSearchRepository
{
    private readonly string _indexName = config["Elasticsearch:IndexName"] ?? "products";

    // Indexes or updates a single product document
    public async Task<bool> IndexProductAsync(Product product)
    {
        try
        {
            var response = await es.IndexAsync(product, i => i.Index(_indexName).Id(product.Id.ToString()));
            if (!response.IsValidResponse)
            {
                logger.LogWarning("Elasticsearch indexing failed for Product {Id}: {Error}", product.Id, response.DebugInformation);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while indexing Product {Id} to Elasticsearch", product.Id);
            return false;
        }
    }

    // Removes a product document from Elasticsearch by id
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var response = await es.DeleteAsync<Product>(id.ToString(), d => d.Index(_indexName));
            return response.IsValidResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete Product {Id} from Elasticsearch", id);
            return false;
        }
    }

    // Performs bulk indexing for a collection of products
    public async Task BulkIndexAsync(IEnumerable<Product> products)
    {
        var response = await es.BulkAsync(b => b
            .Index(_indexName)
            .IndexMany(products, (descriptor, product) => descriptor.Id(product.Id.ToString()))
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Bulk indexing failed: {response.DebugInformation}");
        }

        await es.Indices.RefreshAsync(_indexName);
    }

    // Returns total count of documents in the index
    public async Task<long> CountAsync()
    {
        var response = await es.CountAsync(c => c.Indices(_indexName));
        return response.Count;
    }

    // Executes full-text search with highlighting and field boosting
    public async Task<object> FullTextSearchAsync(string query, int size = 10)
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Size(size)
            .Query(q => q
                .MultiMatch(m => m
                    .Query(query)
                    .Fields(new[] { "name^2", "description" })
                    .Fuzziness(new Fuzziness("AUTO"))
                )
            )
            .Highlight(h => h
                .Fields(hf => hf
                    .Add(new Field("description"), hfd => hfd.PreTags(["<b>"]).PostTags(["</b>"]))
                )
            )
        );

        return new
        {
            total = response.Total,
            tookMs = response.Took,
            results = response.Hits.Select(hit => new
            {
                score = hit.Score,
                product = hit.Source,
                highlights = hit.Highlight
            })
        };
    }

    // Executes structured filtering by category, brand, and numeric price range
    public async Task<object> FilterSearchAsync(string? category, string? brand, decimal? minPrice, decimal? maxPrice)
    {
        var hasFilters = !string.IsNullOrWhiteSpace(category)
                      || !string.IsNullOrWhiteSpace(brand)
                      || minPrice.HasValue
                      || maxPrice.HasValue;

        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Query(q =>
            {
                if (!hasFilters)
                {
                    q.MatchAll();
                    q.MatchAll(m => { });
                }
                else
                {
                    q.Bool(b => b
                        .Filter(f =>
                        {
                            if (!string.IsNullOrWhiteSpace(category))
                                f.Term(t => t.Field(p => p.Category).Value(category));

                            if (!string.IsNullOrWhiteSpace(brand))
                                f.Term(t => t.Field(p => p.Brand).Value(brand));

                            if (minPrice.HasValue || maxPrice.HasValue)
                            {
                                f.Range(r => r
                                    .NumberRange(nr =>
                                    {
                                        nr.Field(p => p.Price);
                                        if (minPrice.HasValue) nr.Gte((double)minPrice.Value);
                                        if (maxPrice.HasValue) nr.Lte((double)maxPrice.Value);
                                    })
                                );
                            }
                        })
                    );
                }
            })
        );

        return new
        {
            total = response.Total,
            tookMs = response.Took,
            products = response.Documents
        };
    }

    // Executes multi-clause boolean search with flexible must, filter, should, and must_not conditions
    public async Task<object> AdvancedSearchAsync(AdvancedSearchRequest req)
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Query(q => q
                .Bool(b =>
                {
                    // MUST clauses (AND - required, affects score)
                    if (!string.IsNullOrWhiteSpace(req.MustQuery))
                        b.Must(m => m.MultiMatch(mm => mm.Query(req.MustQuery).Fields(new[] { "name^2", "description" })));

                    if (!string.IsNullOrWhiteSpace(req.MustBrand))
                        b.Must(m => m.Term(t => t.Field(p => p.Brand).Value(req.MustBrand)));

                    // FILTER clauses (Exact filters - cached, no score impact)
                    if (!string.IsNullOrWhiteSpace(req.FilterCategory))
                        b.Filter(f => f.Term(t => t.Field(p => p.Category).Value(req.FilterCategory)));

                    if (req.MinPrice.HasValue || req.MaxPrice.HasValue)
                    {
                        b.Filter(f => f.Range(r => r
                            .NumberRange(nr =>
                            {
                                nr.Field(p => p.Price);
                                if (req.MinPrice.HasValue) nr.Gte((double)req.MinPrice.Value);
                                if (req.MaxPrice.HasValue) nr.Lte((double)req.MaxPrice.Value);
                            })
                        ));
                    }

                    // SHOULD clauses (OR / Boost - optional, increases score)
                    if (!string.IsNullOrWhiteSpace(req.ShouldTag))
                        b.Should(sh => sh.Term(t => t.Field(p => p.Tags).Value(req.ShouldTag)));

                    if (!string.IsNullOrWhiteSpace(req.ShouldBrand))
                        b.Should(sh => sh.Term(t => t.Field(p => p.Brand).Value(req.ShouldBrand)));

                    // MUST_NOT clauses (NOT - exclusions)
                    if (!string.IsNullOrWhiteSpace(req.ExcludeBrand))
                        b.MustNot(mn => mn.Term(t => t.Field(p => p.Brand).Value(req.ExcludeBrand)));

                    if (!string.IsNullOrWhiteSpace(req.ExcludeTag))
                        b.MustNot(mn => mn.Term(t => t.Field(p => p.Tags).Value(req.ExcludeTag)));
                })
            )
        );

        return new
        {
            total = response.Total,
            tookMs = response.Took,
            results = response.Hits.Select(hit => new
            {
                score = hit.Score,
                product = hit.Source
            })
        };
    }

    // Executes dynamic multi-condition search based on user-configured clauses and fields
    public async Task<object> DynamicSearchAsync(DynamicSearchRequest request)
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Size(request.Size > 0 ? request.Size : 10)
            .Query(q => q
                .Bool(b =>
                {
                    var mustQueries = new List<Query>();
                    var filterQueries = new List<Query>();
                    var shouldQueries = new List<Query>();
                    var mustNotQueries = new List<Query>();

                    foreach (var cond in request.Conditions)
                    {
                        if (string.IsNullOrWhiteSpace(cond.Value)) continue;

                        Query queryClause;
                        var field = cond.Field?.ToLowerInvariant();
                        var val = cond.Value.Trim();

                        if (field == "minprice" && decimal.TryParse(val, out var min))
                        {
                            queryClause = new NumberRangeQuery(new Field("price")) { Gte = (double)min };
                        }
                        else if (field == "maxprice" && decimal.TryParse(val, out var max))
                        {
                            queryClause = new NumberRangeQuery(new Field("price")) { Lte = (double)max };
                        }
                        else if (field == "name")
                        {
                            queryClause = new MatchQuery(new Field("name")) { Query = val };
                        }
                        else if (field == "description")
                        {
                            queryClause = new MatchQuery(new Field("description")) { Query = val };
                        }
                        else if (field == "category")
                        {
                            queryClause = new TermQuery(new Field("category")) { Value = val };
                        }
                        else if (field == "brand")
                        {
                            queryClause = new TermQuery(new Field("brand")) { Value = val };
                        }
                        else if (field == "tags")
                        {
                            queryClause = new TermQuery(new Field("tags")) { Value = val };
                        }
                        else
                        {
                            queryClause = new MultiMatchQuery { Query = val, Fields = new[] { "name^2", "description" } };
                        }

                        var clause = cond.Clause?.ToLowerInvariant();
                        if (clause == "filter")
                        {
                            filterQueries.Add(queryClause);
                        }
                        else if (clause == "should")
                        {
                            shouldQueries.Add(queryClause);
                        }
                        else if (clause == "must_not" || clause == "mustnot")
                        {
                            mustNotQueries.Add(queryClause);
                        }
                        else
                        {
                            mustQueries.Add(queryClause);
                        }
                    }

                    if (mustQueries.Count > 0) b.Must(mustQueries.ToArray());
                    if (filterQueries.Count > 0) b.Filter(filterQueries.ToArray());
                    if (shouldQueries.Count > 0) b.Should(shouldQueries.ToArray());
                    if (mustNotQueries.Count > 0) b.MustNot(mustNotQueries.ToArray());

                    if (mustQueries.Count == 0 && filterQueries.Count == 0 && shouldQueries.Count == 0 && mustNotQueries.Count == 0)
                    {
                        b.Must(new MatchAllQuery());
                        b.Must(m => m.MatchAll(ma => { }));
                    }
                })
            )
        );

        return new
        {
            total = response.Total,
            tookMs = response.Took,
            results = response.Hits.Select(hit => new
            {
                score = hit.Score,
                product = hit.Source
            })
        };
    }

    // Queries Edge N-Gram tokenized autocomplete field for fast prefix matching
    public async Task<object> AutocompleteAsync(string prefix, int size = 5)
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Size(size)
            .Query(q => q.Match(m => m.Field(new Field("name.autocomplete")).Query(prefix)))
        );

        return new
        {
            suggestions = response.Documents.Select(d => new
            {
                d.Id,
                d.Name,
                d.Category,
                d.Price
            })
        };
    }

    // Queries fuzzy match for spelling typo correction
    public async Task<object> FuzzySearchAsync(string typo, int size = 5)
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Size(size)
            .Query(q => q
                .Fuzzy(f => f
                    .Field(p => p.Name)
                    .Value(typo)
                    .Fuzziness(new Fuzziness("AUTO"))
                )
            )
        );

        return new
        {
            total = response.Total,
            tookMs = response.Took,
            products = response.Documents
        };
    }

    // Aggregates product distributions and price statistics
    public async Task<object> GetStatsAsync()
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Size(0)
            .Aggregations(a => a
                .Add("categories", agg => agg.Terms(t => t.Field(p => p.Category)))
                .Add("brands", agg => agg.Terms(t => t.Field(p => p.Brand)))
                .Add("price_stats", agg => agg.Stats(st => st.Field(p => p.Price)))
            )
        );

        var categoriesAgg = response.Aggregations?.GetStringTerms("categories");
        var brandsAgg = response.Aggregations?.GetStringTerms("brands");
        var priceStatsAgg = response.Aggregations?.GetStats("price_stats");

        return new
        {
            totalProducts = response.Total,
            categories = categoriesAgg?.Buckets.Select(b => new { category = b.Key, count = b.DocCount }),
            brands = brandsAgg?.Buckets.Select(b => new { brand = b.Key, count = b.DocCount }),
            priceStats = priceStatsAgg != null ? new
            {
                min = priceStatsAgg.Min,
                max = priceStatsAgg.Max,
                avg = priceStatsAgg.Avg,
                sum = priceStatsAgg.Sum,
                count = priceStatsAgg.Count
            } : null
        };
    }
}