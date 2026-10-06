using EcomSearchApi.Models;
using EcomSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomSearchApi.Controllers;

// Controller for product CRUD operations
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    // Retrieves all products
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await productService.GetAllAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Retrieves a single product by identifier
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await productService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Creates a new product
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        try
        {
            var created = await productService.CreateAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Updates an existing product
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        try
        {
            var updated = await productService.UpdateAsync(id, product);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Deletes a product by identifier
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await productService.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
