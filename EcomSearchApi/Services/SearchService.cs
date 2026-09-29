using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using EcomSearchApi.Models;

namespace EcomSearchApi.Services;

public class SearchService(ElasticsearchClient es, IConfiguration config) : ISearchService
{
    private readonly string _indexName = config["Elasticsearch:IndexName"] ?? "products";

    // 1. Full-Text BM25 Search + Highlighting + Name^2 Boost
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
                    .Add(new Field("description"), hfd => hfd
                        .PreTags(["<b>"])
                        .PostTags(["</b>"])
                    )
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

    // 2. Filter Search: Chạy trong Bool Filter Context, Score = 0, Bitset Cache RAM siêu tốc
    public async Task<object> FilterSearchAsync(string? category, string? brand, decimal? minPrice, decimal? maxPrice)
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Query(q => q
                .Bool(b => b
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

    // 3. Advanced Compound Bool: Must (BM25) + Filter (Category) + Should (Premium boost) + MustNot (Exclude brand)
    public async Task<object> AdvancedSearchAsync(string? query, string? category, string? excludeBrand)
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Query(q => q
                .Bool(b =>
                {
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        b.Must(m => m.MultiMatch(mm => mm.Query(query).Fields(new[] { "name^2", "description" })));
                    }

                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        b.Filter(f => f.Term(t => t.Field(p => p.Category).Value(category)));
                    }

                    b.Should(sh => sh.Term(t => t.Field(p => p.Tags).Value("premium")));

                    if (!string.IsNullOrWhiteSpace(excludeBrand))
                    {
                        b.MustNot(mn => mn.Term(t => t.Field(p => p.Brand).Value(excludeBrand)));
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

    // 4. Search-As-You-Type Autocomplete: Edge N-Gram Match Query
    public async Task<object> AutocompleteAsync(string prefix, int size = 5)
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Size(size)
            .Query(q => q
                .Match(m => m
                    .Field("name.autocomplete")
                    .Query(prefix)
                )
            )
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

    // 5. Fuzzy Typo-Tolerant Search: Levenshtein distance AUTO
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
            query = typo,
            total = response.Total,
            results = response.Documents
        };
    }

    // 6. Real-time Aggregations: Size(0), Terms Aggs & Metric Stats
    public async Task<object> GetStatsAsync()
    {
        var response = await es.SearchAsync<Product>(s => s
            .Index(_indexName)
            .Size(0)
            .Aggregations(a => a
                .Add("by_category", agg => agg
                    .Terms(t => t.Field(p => p.Category))
                    .Aggregations(sub => sub
                        .Add("avg_price", sa => sa.Avg(avg => avg.Field(p => p.Price)))
                    )
                )
                .Add("by_brand", agg => agg
                    .Terms(t => t.Field(p => p.Brand))
                    .Aggregations(sub => sub
                        .Add("avg_price", sa => sa.Avg(avg => avg.Field(p => p.Price)))
                    )
                )
                .Add("price_stats", agg => agg
                    .Stats(st => st.Field(p => p.Price))
                )
            )
        );

        return new
        {
            tookMs = response.Took,
            aggregations = response.Aggregations
        };
    }
}
