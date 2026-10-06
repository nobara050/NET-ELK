import { useState } from 'react';
import { resetAll } from '../api/resetApi';

export default function ResetButton({ onResetSuccess }) {
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  async function handleReset() {
    if (!window.confirm('Reset database and re-seed Elasticsearch index?')) {
      return;
    }
    setLoading(true);
    setMessage('');
    try {
      const res = await resetAll();
      setMessage(res?.message || 'Reset completed successfully.');
      if (onResetSuccess) {
        onResetSuccess();
      }
    } catch (err) {
      setMessage(`Error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ margin: '12px 0' }}>
      <button onClick={handleReset} disabled={loading} style={{ padding: '6px 12px' }}>
        {loading ? 'Resetting System...' : 'Reset Database & ES Index'}
      </button>
      {message && <span style={{ marginLeft: '12px' }}>{message}</span>}
    </div>
  );
}
