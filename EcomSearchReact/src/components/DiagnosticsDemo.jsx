import { useState } from 'react';
import {
  triggerBusinessError,
  simulateException,
  triggerBurstErrors
} from '../api/diagnosticsApi';

export default function DiagnosticsDemo() {
  const [errorMessage, setErrorMessage] = useState('Payment failed due to timeout');
  const [orderId, setOrderId] = useState('ORD-999');
  const [serviceName, setServiceName] = useState('OrderService');
  const [burstCount, setBurstCount] = useState(5);

  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);

  async function handleBusinessError() {
    setLoading(true);
    setResult(null);
    try {
      const data = await triggerBusinessError(errorMessage, orderId);
      setResult({ type: 'Business Error Sent', data });
    } catch (err) {
      setResult({ type: 'Error Response', error: err.message });
    } finally {
      setLoading(false);
    }
  }

  async function handleSimulateException() {
    setLoading(true);
    setResult(null);
    try {
      const data = await simulateException(serviceName);
      setResult({ type: 'Exception Simulation Response', data });
    } catch (err) {
      setResult({ type: 'Server Exception Caught (Expected 500)', error: err.message });
    } finally {
      setLoading(false);
    }
  }

  async function handleBurstErrors() {
    setLoading(true);
    setResult(null);
    try {
      const data = await triggerBurstErrors(burstCount);
      setResult({ type: 'Burst Errors Sent', data });
    } catch (err) {
      setResult({ type: 'Error Response', error: err.message });
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ margin: '16px 0' }}>
      <h2>ELK Stack Diagnostics & Log Generator</h2>
      <p style={{ margin: '8px 0' }}>
        Use these actions to generate log events into Serilog, Logstash, and Elasticsearch to verify Kibana dashboards.
      </p>

      <fieldset style={{ padding: '12px', margin: '12px 0' }}>
        <legend>Trigger Business Error Log</legend>
        <div style={{ margin: '8px 0' }}>
          <label style={{ marginRight: '12px' }}>
            Message:
            <input
              type="text"
              value={errorMessage}
              onChange={(e) => setErrorMessage(e.target.value)}
              style={{ marginLeft: '6px', padding: '4px' }}
            />
          </label>
          <label style={{ marginRight: '12px' }}>
            Order ID:
            <input
              type="text"
              value={orderId}
              onChange={(e) => setOrderId(e.target.value)}
              style={{ marginLeft: '6px', padding: '4px' }}
            />
          </label>
          <button onClick={handleBusinessError} disabled={loading} style={{ padding: '4px 12px' }}>
            Send Error Log
          </button>
        </div>
      </fieldset>

      <fieldset style={{ padding: '12px', margin: '12px 0' }}>
        <legend>Simulate Unhandled Exception (500 Error)</legend>
        <div style={{ margin: '8px 0' }}>
          <label style={{ marginRight: '12px' }}>
            Service Name:
            <input
              type="text"
              value={serviceName}
              onChange={(e) => setServiceName(e.target.value)}
              style={{ marginLeft: '6px', padding: '4px' }}
            />
          </label>
          <button onClick={handleSimulateException} disabled={loading} style={{ padding: '4px 12px' }}>
            Trigger Exception
          </button>
        </div>
      </fieldset>

      <fieldset style={{ padding: '12px', margin: '12px 0' }}>
        <legend>Trigger Burst Errors (Consecutive Rate Test)</legend>
        <div style={{ margin: '8px 0' }}>
          <label style={{ marginRight: '12px' }}>
            Count:
            <input
              type="number"
              value={burstCount}
              onChange={(e) => setBurstCount(Number(e.target.value))}
              style={{ marginLeft: '6px', padding: '4px', width: '60px' }}
            />
          </label>
          <button onClick={handleBurstErrors} disabled={loading} style={{ padding: '4px 12px' }}>
            Send Burst Logs
          </button>
        </div>
      </fieldset>

      {loading && <p style={{ margin: '8px 0' }}>Sending request...</p>}

      {result && (
        <fieldset style={{ padding: '12px', margin: '16px 0' }}>
          <legend>{result.type}</legend>
          <pre style={{ margin: '8px 0', padding: '8px', overflow: 'auto' }}>
            {JSON.stringify(result.data || result.error, null, 2)}
          </pre>
        </fieldset>
      )}
    </div>
  );
}
