import { useState, useEffect } from 'react';
import { getStats } from '../api/searchApi';

export default function StatsDemo() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  async function fetchStats() {
    setLoading(true);
    setError('');
    try {
      const data = await getStats();
      setStats(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchStats();
  }, []);

  return (
    <div style={{ margin: '16px 0' }}>
      <h2>Elasticsearch Aggregations & Statistics</h2>

      <div style={{ margin: '12px 0' }}>
        <button onClick={fetchStats} disabled={loading} style={{ padding: '6px 12px' }}>
          {loading ? 'Refreshing...' : 'Refresh Statistics'}
        </button>
      </div>

      {loading && <p style={{ margin: '8px 0' }}>Loading aggregations...</p>}
      {error && <p style={{ margin: '8px 0' }}>Error: {error}</p>}

      {stats && (
        <div style={{ margin: '16px 0' }}>
          <p style={{ margin: '8px 0' }}>
            <strong>Total Indexed Products:</strong> {stats.totalProducts ?? 0}
          </p>

          <fieldset style={{ padding: '12px', margin: '12px 0' }}>
            <legend>Price Metrics (Stats Aggregation)</legend>
            {stats.priceStats ? (
              <table border="1" cellPadding="6" cellSpacing="0" style={{ margin: '8px 0' }}>
                <thead>
                  <tr>
                    <th>Metric</th>
                    <th>Value</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>Count</td>
                    <td>{stats.priceStats.count}</td>
                  </tr>
                  <tr>
                    <td>Min Price</td>
                    <td>${stats.priceStats.min}</td>
                  </tr>
                  <tr>
                    <td>Max Price</td>
                    <td>${stats.priceStats.max}</td>
                  </tr>
                  <tr>
                    <td>Average Price</td>
                    <td>${Number(stats.priceStats.avg).toFixed(2)}</td>
                  </tr>
                  <tr>
                    <td>Sum</td>
                    <td>${Number(stats.priceStats.sum).toFixed(2)}</td>
                  </tr>
                </tbody>
              </table>
            ) : (
              <p style={{ margin: '8px 0' }}>No price stats available</p>
            )}
          </fieldset>

          <div style={{ display: 'flex', gap: '24px', margin: '12px 0' }}>
            <fieldset style={{ flex: 1, padding: '12px' }}>
              <legend>Categories Distribution</legend>
              <table border="1" cellPadding="6" cellSpacing="0" style={{ width: '100%', margin: '8px 0' }}>
                <thead>
                  <tr>
                    <th>Category</th>
                    <th>Count</th>
                  </tr>
                </thead>
                <tbody>
                  {stats.categories && stats.categories.length > 0 ? (
                    stats.categories.map((c, i) => (
                      <tr key={i}>
                        <td>{c.category}</td>
                        <td>{c.count}</td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan="2" style={{ textAlign: 'center' }}>
                        No categories found
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </fieldset>

            <fieldset style={{ flex: 1, padding: '12px' }}>
              <legend>Brands Distribution</legend>
              <table border="1" cellPadding="6" cellSpacing="0" style={{ width: '100%', margin: '8px 0' }}>
                <thead>
                  <tr>
                    <th>Brand</th>
                    <th>Count</th>
                  </tr>
                </thead>
                <tbody>
                  {stats.brands && stats.brands.length > 0 ? (
                    stats.brands.map((b, i) => (
                      <tr key={i}>
                        <td>{b.brand}</td>
                        <td>{b.count}</td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan="2" style={{ textAlign: 'center' }}>
                        No brands found
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </fieldset>
          </div>

          <details style={{ margin: '12px 0' }}>
            <summary>View Raw JSON</summary>
            <pre style={{ margin: '8px 0', padding: '8px', overflow: 'auto', maxHeight: '300px' }}>
              {JSON.stringify(stats, null, 2)}
            </pre>
          </details>
        </div>
      )}
    </div>
  );
}
