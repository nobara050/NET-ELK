import { useState } from 'react';
import SearchDemo from './components/SearchDemo';
import StatsDemo from './components/StatsDemo';
import ProductsDemo from './components/ProductsDemo';
import DiagnosticsDemo from './components/DiagnosticsDemo';
import ResetButton from './components/ResetButton';

export default function App() {
  const [activeTab, setActiveTab] = useState('search');
  const [refreshKey, setRefreshKey] = useState(0);

  function handleResetSuccess() {
    setRefreshKey((prev) => prev + 1);
  }

  return (
    <div style={{ maxWidth: '1200px', margin: '0 auto', padding: '16px' }}>
      <header style={{ margin: '16px 0' }}>
        <h1 style={{ margin: '8px 0' }}>Elasticsearch E-Commerce Demo</h1>
        <ResetButton onResetSuccess={handleResetSuccess} />
      </header>

      <nav style={{ margin: '16px 0' }}>
        <button
          onClick={() => setActiveTab('search')}
          style={{ padding: '8px 16px', marginRight: '8px', fontWeight: activeTab === 'search' ? 'bold' : 'normal' }}
        >
          Search Demo
        </button>
        <button
          onClick={() => setActiveTab('stats')}
          style={{ padding: '8px 16px', marginRight: '8px', fontWeight: activeTab === 'stats' ? 'bold' : 'normal' }}
        >
          Aggregations & Stats
        </button>
        <button
          onClick={() => setActiveTab('products')}
          style={{ padding: '8px 16px', marginRight: '8px', fontWeight: activeTab === 'products' ? 'bold' : 'normal' }}
        >
          Product Management
        </button>
        <button
          onClick={() => setActiveTab('diagnostics')}
          style={{ padding: '8px 16px', fontWeight: activeTab === 'diagnostics' ? 'bold' : 'normal' }}
        >
          ELK Diagnostics
        </button>
      </nav>

      <hr style={{ margin: '16px 0' }} />

      <main key={`${activeTab}-${refreshKey}`}>
        {activeTab === 'search' && <SearchDemo key="search" />}
        {activeTab === 'stats' && <StatsDemo key="stats" />}
        {activeTab === 'products' && <ProductsDemo key="products" />}
        {activeTab === 'diagnostics' && <DiagnosticsDemo key="diagnostics" />}
      </main>
    </div>
  );
}
