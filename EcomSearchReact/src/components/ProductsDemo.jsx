import { useState, useEffect } from 'react';
import {
  getAllProducts,
  createProduct,
  updateProduct,
  deleteProduct
} from '../api/productsApi';

const emptyForm = {
  name: '',
  description: '',
  category: '',
  brand: '',
  price: '',
  stock: '',
  tags: ''
};

export default function ProductsDemo() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [formData, setFormData] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [saving, setSaving] = useState(false);
  const [statusMessage, setStatusMessage] = useState('');

  async function loadProducts() {
    setLoading(true);
    setError('');
    try {
      const data = await getAllProducts();
      setProducts(data || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadProducts();
  }, []);

  function handleInputChange(e) {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  }

  function startEdit(p) {
    setEditingId(p.id);
    setFormData({
      name: p.name || '',
      description: p.description || '',
      category: p.category || '',
      brand: p.brand || '',
      price: p.price ?? '',
      stock: p.stock ?? '',
      tags: Array.isArray(p.tags) ? p.tags.join(', ') : ''
    });
    setStatusMessage('');
  }

  function cancelEdit() {
    setEditingId(null);
    setFormData(emptyForm);
    setStatusMessage('');
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setStatusMessage('');
    setError('');

    const payload = {
      name: formData.name,
      description: formData.description,
      category: formData.category,
      brand: formData.brand,
      price: parseFloat(formData.price) || 0,
      stock: parseInt(formData.stock, 10) || 0,
      tags: formData.tags
        ? formData.tags.split(',').map((t) => t.trim()).filter(Boolean)
        : []
    };

    try {
      if (editingId) {
        payload.id = editingId;
        await updateProduct(editingId, payload);
        setStatusMessage(`Product #${editingId} updated successfully.`);
      } else {
        const created = await createProduct(payload);
        setStatusMessage(`Product created with ID: ${created?.id || 'OK'}`);
      }
      cancelEdit();
      await loadProducts();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!window.confirm(`Delete product #${id}?`)) {
      return;
    }
    setStatusMessage('');
    setError('');
    try {
      await deleteProduct(id);
      setStatusMessage(`Product #${id} deleted.`);
      await loadProducts();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div style={{ margin: '16px 0' }}>
      <h2>PostgreSQL Product Management (Synced with ES)</h2>

      <div style={{ margin: '12px 0' }}>
        <button onClick={loadProducts} disabled={loading} style={{ padding: '6px 12px' }}>
          {loading ? 'Refreshing...' : 'Refresh List'}
        </button>
      </div>

      <fieldset style={{ padding: '12px', margin: '16px 0' }}>
        <legend>{editingId ? `Edit Product #${editingId}` : 'Add New Product'}</legend>
        <form onSubmit={handleSubmit}>
          <div style={{ margin: '8px 0' }}>
            <label style={{ marginRight: '16px' }}>
              Name:
              <input
                type="text"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
                required
                style={{ marginLeft: '6px', padding: '4px' }}
              />
            </label>
            <label style={{ marginRight: '16px' }}>
              Category:
              <input
                type="text"
                name="category"
                value={formData.category}
                onChange={handleInputChange}
                required
                style={{ marginLeft: '6px', padding: '4px' }}
              />
            </label>
            <label>
              Brand:
              <input
                type="text"
                name="brand"
                value={formData.brand}
                onChange={handleInputChange}
                required
                style={{ marginLeft: '6px', padding: '4px' }}
              />
            </label>
          </div>

          <div style={{ margin: '8px 0' }}>
            <label style={{ marginRight: '16px' }}>
              Price:
              <input
                type="number"
                step="0.01"
                name="price"
                value={formData.price}
                onChange={handleInputChange}
                required
                style={{ marginLeft: '6px', padding: '4px', width: '80px' }}
              />
            </label>
            <label style={{ marginRight: '16px' }}>
              Stock:
              <input
                type="number"
                name="stock"
                value={formData.stock}
                onChange={handleInputChange}
                required
                style={{ marginLeft: '6px', padding: '4px', width: '80px' }}
              />
            </label>
            <label>
              Tags (comma separated):
              <input
                type="text"
                name="tags"
                value={formData.tags}
                onChange={handleInputChange}
                placeholder="tag1, tag2"
                style={{ marginLeft: '6px', padding: '4px' }}
              />
            </label>
          </div>

          <div style={{ margin: '8px 0' }}>
            <label style={{ display: 'block', margin: '4px 0' }}>Description:</label>
            <textarea
              name="description"
              rows={2}
              value={formData.description}
              onChange={handleInputChange}
              style={{ width: '100%', padding: '4px', boxSizing: 'border-box' }}
            />
          </div>

          <div style={{ margin: '8px 0' }}>
            <button type="submit" disabled={saving} style={{ padding: '6px 14px', marginRight: '8px' }}>
              {saving ? 'Saving...' : editingId ? 'Update Product' : 'Create Product'}
            </button>
            {editingId && (
              <button type="button" onClick={cancelEdit} style={{ padding: '6px 14px' }}>
                Cancel
              </button>
            )}
          </div>
        </form>
      </fieldset>

      {statusMessage && <p style={{ margin: '8px 0' }}>{statusMessage}</p>}
      {error && <p style={{ margin: '8px 0' }}>Error: {error}</p>}
      {loading && <p style={{ margin: '8px 0' }}>Loading products...</p>}

      <div style={{ margin: '16px 0' }}>
        <p style={{ margin: '8px 0' }}>
          <strong>Total products in database:</strong> {products.length}
        </p>

        <table border="1" cellPadding="6" cellSpacing="0" style={{ width: '100%', margin: '12px 0' }}>
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Category</th>
              <th>Brand</th>
              <th>Price</th>
              <th>Stock</th>
              <th>Tags</th>
              <th>Actions</th>
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
              products.map((p) => (
                <tr key={p.id}>
                  <td>{p.id}</td>
                  <td>{p.name}</td>
                  <td>{p.category}</td>
                  <td>{p.brand}</td>
                  <td>${p.price}</td>
                  <td>{p.stock}</td>
                  <td>{Array.isArray(p.tags) ? p.tags.join(', ') : ''}</td>
                  <td>
                    <button
                      onClick={() => startEdit(p)}
                      style={{ padding: '2px 8px', marginRight: '6px' }}
                    >
                      Edit
                    </button>
                    <button onClick={() => handleDelete(p.id)} style={{ padding: '2px 8px' }}>
                      Delete
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
