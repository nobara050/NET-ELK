import { useState, useEffect } from 'react';
import {
  fullTextSearch,
  filterSearch,
  dynamicSearch,
  autocomplete,
  fuzzySearch
} from '../api/searchApi';

export default function SearchDemo() {
  const [mode, setMode] = useState('fulltext');

  // Query states
  const [fullTextQuery, setFullTextQuery] = useState('phone');
  const [size, setSize] = useState(10);

  const [prefixQuery, setPrefixQuery] = useState('sam');
  const [suggestions, setSuggestions] = useState([]);

  const [fuzzyQuery, setFuzzyQuery] = useState('iphne');

  const [filterCategory, setFilterCategory] = useState('');
  const [filterBrand, setFilterBrand] = useState('');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');

  // Dynamic conditions for Boolean Search (Add / Remove rows)
  const [conditions, setConditions] = useState([
    { id: 1, clause: 'must', field: 'name', value: '' },
    { id: 2, clause: 'filter', field: 'category', value: '' }
  ]);

  // Execution states
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [searchResponse, setSearchResponse] = useState(null);

  // Autocomplete live suggestion effect
  useEffect(() => {
    if (mode !== 'autocomplete') return;
    if (!prefixQuery.trim()) {
      setSuggestions([]);
      return;
    }

    let isMounted = true;
    autocomplete(prefixQuery.trim(), 5)
      .then((data) => {
        if (isMounted) {
          setSuggestions(data?.suggestions || []);
        }
      })
      .catch((err) => {
        if (isMounted) {
          setError(err.message);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [mode, prefixQuery]);

  function handleAddCondition() {
    setConditions((prev) => [
      ...prev,
      { id: Date.now(), clause: 'must', field: 'name', value: '' }
    ]);
  }

  function handleRemoveCondition(id) {
    setConditions((prev) => prev.filter((c) => c.id !== id));
  }

  function handleConditionChange(id, key, val) {
    setConditions((prev) =>
      prev.map((c) => (c.id === id ? { ...c, [key]: val } : c))
    );
  }

  async function handleSearch(e) {
    if (e) e.preventDefault();
    setLoading(true);
    setError('');
    setSearchResponse(null);

    try {
      let data = null;
      if (mode === 'fulltext') {
        data = await fullTextSearch(fullTextQuery, size);
      } else if (mode === 'fuzzy') {
        data = await fuzzySearch(fuzzyQuery, size);
      } else if (mode === 'filter') {
        data = await filterSearch({
          category: filterCategory,
          brand: filterBrand,
          minPrice: minPrice || undefined,
          maxPrice: maxPrice || undefined
        });
      } else if (mode === 'advanced') {
        const validConditions = conditions
          .filter((c) => c.value && c.value.trim() !== '')
          .map(({ clause, field, value }) => ({ clause, field, value }));
        data = await dynamicSearch(validConditions, size);
      }
      setSearchResponse(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  // Parse product rows from different response formats
  const products = [];
  if (searchResponse) {
    if (Array.isArray(searchResponse.results)) {
      searchResponse.results.forEach((item) => {
        products.push({
          score: item.score,
          product: item.product,
          highlights: item.highlights
        });
      });
    } else if (Array.isArray(searchResponse.products)) {
      searchResponse.products.forEach((prod) => {
        products.push({
          score: null,
          product: prod,
          highlights: null
        });
      });
    }
  }

  function handleModeChange(newMode) {
    setMode(newMode);
    setSearchResponse(null);
    setError('');
  }

  return (
    <div style={{ margin: '16px 0' }}>
      <h2>Elasticsearch Search Demo</h2>

      <div style={{ display: 'flex', gap: '20px', flexWrap: 'wrap', alignItems: 'center', margin: '12px 0' }}>
        <label>
          <input
            type="radio"
            name="searchMode"
            value="fulltext"
            checked={mode === 'fulltext'}
            onChange={() => handleModeChange('fulltext')}
          />
          Full-Text Search
        </label>
        <label>
          <input
            type="radio"
            name="searchMode"
            value="autocomplete"
            checked={mode === 'autocomplete'}
            onChange={() => handleModeChange('autocomplete')}
          />
          Autocomplete (Edge N-Gram)
        </label>
        <label>
          <input
            type="radio"
            name="searchMode"
            value="fuzzy"
            checked={mode === 'fuzzy'}
            onChange={() => handleModeChange('fuzzy')}
          />
          Fuzzy Search (Typo Tolerance)
        </label>
        <label>
          <input
            type="radio"
            name="searchMode"
            value="filter"
            checked={mode === 'filter'}
            onChange={() => handleModeChange('filter')}
          />
          Filter Search (Bool Query)
        </label>
        <label>
          <input
            type="radio"
            name="searchMode"
            value="advanced"
            checked={mode === 'advanced'}
            onChange={() => handleModeChange('advanced')}
          />
          Advanced (Must/Filter/MustNot)
        </label>
      </div>

      {mode === 'fulltext' && (
        <form onSubmit={handleSearch} style={{ margin: '12px 0' }}>
          <fieldset style={{ padding: '12px', margin: '8px 0' }}>
            <legend>Full-Text Query</legend>
            <div style={{ margin: '8px 0' }}>
              <label>
                Query:
                <input
                  type="text"
                  value={fullTextQuery}
                  onChange={(e) => setFullTextQuery(e.target.value)}
                  style={{ margin: '0 8px', padding: '4px' }}
                />
              </label>
              <label>
                Size:
                <input
                  type="number"
                  value={size}
                  onChange={(e) => setSize(Number(e.target.value))}
                  style={{ margin: '0 8px', padding: '4px', width: '60px' }}
                />
              </label>
              <button type="submit" disabled={loading} style={{ padding: '4px 12px' }}>
                Search
              </button>
            </div>
          </fieldset>
        </form>
      )}

      {mode === 'autocomplete' && (
        <fieldset style={{ padding: '12px', margin: '8px 0' }}>
          <legend>Autocomplete Prefix</legend>
          <div style={{ margin: '8px 0' }}>
            <label>
              Type prefix:
              <input
                type="text"
                value={prefixQuery}
                onChange={(e) => setPrefixQuery(e.target.value)}
                placeholder="e.g. sam, iph, lap"
                style={{ margin: '0 8px', padding: '4px' }}
              />
            </label>
          </div>
          <div>
            <strong>Suggestions:</strong>
            {suggestions.length === 0 ? (
              <p style={{ margin: '8px 0' }}>No suggestions</p>
            ) : (
              <ul style={{ margin: '8px 0', paddingLeft: '24px' }}>
                {suggestions.map((s) => (
                  <li key={s.id || s.Id}>
                    {s.name || s.Name} ({s.category || s.Category} - ${s.price ?? s.Price})
                  </li>
                ))}
              </ul>
            )}
          </div>
        </fieldset>
      )}

      {mode === 'fuzzy' && (
        <form onSubmit={handleSearch} style={{ margin: '12px 0' }}>
          <fieldset style={{ padding: '12px', margin: '8px 0' }}>
            <legend>Fuzzy Query (Typo Tolerance)</legend>
            <div style={{ margin: '8px 0' }}>
              <label>
                Typo query:
                <input
                  type="text"
                  value={fuzzyQuery}
                  onChange={(e) => setFuzzyQuery(e.target.value)}
                  placeholder="e.g. iphne, samung, macbok"
                  style={{ margin: '0 8px', padding: '4px' }}
                />
              </label>
              <button type="submit" disabled={loading} style={{ padding: '4px 12px' }}>
                Search
              </button>
            </div>
          </fieldset>
        </form>
      )}

      {mode === 'filter' && (
        <form onSubmit={handleSearch} style={{ margin: '12px 0' }}>
          <fieldset style={{ padding: '12px', margin: '8px 0' }}>
            <legend>Filter Parameters</legend>
            <div style={{ margin: '8px 0' }}>
              <label style={{ marginRight: '12px' }}>
                Category:
                <input
                  type="text"
                  value={filterCategory}
                  onChange={(e) => setFilterCategory(e.target.value)}
                  placeholder="e.g. Electronics"
                  style={{ marginLeft: '4px', padding: '4px' }}
                />
              </label>
              <label style={{ marginRight: '12px' }}>
                Brand:
                <input
                  type="text"
                  value={filterBrand}
                  onChange={(e) => setFilterBrand(e.target.value)}
                  placeholder="e.g. Apple"
                  style={{ marginLeft: '4px', padding: '4px' }}
                />
              </label>
            </div>
            <div style={{ margin: '8px 0' }}>
              <label style={{ marginRight: '12px' }}>
                Min Price:
                <input
                  type="number"
                  value={minPrice}
                  onChange={(e) => setMinPrice(e.target.value)}
                  style={{ marginLeft: '4px', padding: '4px', width: '80px' }}
                />
              </label>
              <label style={{ marginRight: '12px' }}>
                Max Price:
                <input
                  type="number"
                  value={maxPrice}
                  onChange={(e) => setMaxPrice(e.target.value)}
                  style={{ marginLeft: '4px', padding: '4px', width: '80px' }}
                />
              </label>
              <button type="submit" disabled={loading} style={{ padding: '4px 12px' }}>
                Apply Filters
              </button>
            </div>
          </fieldset>
        </form>
      )}

      {mode === 'advanced' && (
        <form onSubmit={handleSearch} style={{ margin: '12px 0' }}>
          <fieldset style={{ padding: '12px', margin: '8px 0' }}>
            <legend>Dynamic Multi-Clause Boolean Search (Add / Remove Fields)</legend>

            <div style={{ margin: '8px 0' }}>
              <p style={{ margin: '4px 0 8px 0' }}>
                Build your Elasticsearch bool query dynamically by adding clauses (MUST, FILTER, SHOULD, MUST_NOT):
              </p>

              {conditions.map((item, idx) => (
                <div
                  key={item.id}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '12px',
                    margin: '8px 0',
                    flexWrap: 'wrap'
                  }}
                >
                  <label>
                    Clause:
                    <select
                      value={item.clause}
                      onChange={(e) => handleConditionChange(item.id, 'clause', e.target.value)}
                      style={{ marginLeft: '6px', padding: '4px' }}
                    >
                      <option value="must">MUST (AND - Required)</option>
                      <option value="filter">FILTER (Exact - Cached)</option>
                      <option value="should">SHOULD (OR - Boost Score)</option>
                      <option value="must_not">MUST NOT (Exclude)</option>
                    </select>
                  </label>

                  <label>
                    Field:
                    <select
                      value={item.field}
                      onChange={(e) => handleConditionChange(item.id, 'field', e.target.value)}
                      style={{ marginLeft: '6px', padding: '4px' }}
                    >
                      <option value="name">Name (Full-Text)</option>
                      <option value="description">Description (Full-Text)</option>
                      <option value="category">Category (Term)</option>
                      <option value="brand">Brand (Term)</option>
                      <option value="tags">Tags (Term)</option>
                      <option value="minPrice">Min Price (&gt;=)</option>
                      <option value="maxPrice">Max Price (&lt;=)</option>
                    </select>
                  </label>

                  <label>
                    Value:
                    <input
                      type="text"
                      value={item.value}
                      onChange={(e) => handleConditionChange(item.id, 'value', e.target.value)}
                      placeholder="e.g. phone, Sony, 100"
                      style={{ marginLeft: '6px', padding: '4px' }}
                    />
                  </label>

                  <button
                    type="button"
                    onClick={() => handleRemoveCondition(item.id)}
                    disabled={conditions.length === 1}
                    style={{ padding: '4px 10px' }}
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>

            <div style={{ display: 'flex', gap: '12px', alignItems: 'center', margin: '14px 0 6px 0' }}>
              <button
                type="button"
                onClick={handleAddCondition}
                style={{ padding: '6px 14px' }}
              >
                + Add Condition
              </button>

              <button
                type="submit"
                disabled={loading}
                style={{ padding: '6px 18px', fontWeight: 'bold' }}
              >
                Execute Dynamic Query
              </button>
            </div>
          </fieldset>
        </form>
      )}

      {loading && <p style={{ margin: '8px 0' }}>Loading search results...</p>}
      {error && <p style={{ margin: '8px 0' }}>Error: {error}</p>}

      {searchResponse && (
        <div style={{ margin: '16px 0' }}>
          <div style={{ margin: '8px 0' }}>
            <strong>Total documents:</strong> {searchResponse.total ?? products.length} |{' '}
            <strong>Took:</strong> {searchResponse.tookMs ?? 'N/A'} ms
          </div>

          <table border="1" cellPadding="6" cellSpacing="0" style={{ width: '100%', margin: '12px 0' }}>
            <thead>
              <tr>
                <th>ID</th>
                <th>Score</th>
                <th>Name</th>
                <th>Category</th>
                <th>Brand</th>
                <th>Price</th>
                <th>Stock</th>
                <th>Description</th>
              </tr>
            </thead>
            <tbody>
              {products.length === 0 ? (
                <tr>
                  <td colSpan="8" style={{ textAlign: 'center' }}>
                    No products found
                  </td>
                </tr>
              ) : (
                products.map((item, idx) => {
                  const p = item.product || {};
                  const highlightDesc = item.highlights?.description?.[0];
                  return (
                    <tr key={p.id || p.Id || idx}>
                      <td>{p.id ?? p.Id}</td>
                      <td>{item.score !== null && item.score !== undefined ? Number(item.score).toFixed(3) : '-'}</td>
                      <td>{p.name ?? p.Name}</td>
                      <td>{p.category ?? p.Category}</td>
                      <td>{p.brand ?? p.Brand}</td>
                      <td>${p.price ?? p.Price}</td>
                      <td>{p.stock ?? p.Stock ?? p.stockQuantity ?? p.StockQuantity}</td>
                      <td>
                        {highlightDesc ? (
                          <span dangerouslySetInnerHTML={{ __html: highlightDesc }} />
                        ) : (
                          p.description ?? p.Description
                        )}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>

          <details style={{ margin: '12px 0' }}>
            <summary>View Raw JSON Response</summary>
            <pre style={{ margin: '8px 0', padding: '8px', overflow: 'auto', maxHeight: '300px' }}>
              {JSON.stringify(searchResponse, null, 2)}
            </pre>
          </details>
        </div>
      )}
    </div>
  );
}
