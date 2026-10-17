using System.Security.Claims;
using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/finance")]
public sealed class FinanceController(IFinanceService financeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOverview([FromQuery] string? month, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try
        {
            return Ok(await financeService.GetOverview(userId, month ?? string.Empty, cancellationToken));
        }
        catch (FinanceValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> AddTransaction([FromBody] FinanceTransactionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try
        {
            return Ok(await financeService.AddTransaction(userId, request, cancellationToken));
        }
        catch (FinanceValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("transactions/{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return await financeService.DeleteTransaction(userId, id, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpPut("budgets")]
    public async Task<IActionResult> UpsertBudget([FromBody] FinanceBudgetRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try
        {
            return Ok(await financeService.UpsertBudget(userId, request, cancellationToken));
        }
        catch (FinanceValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out userId);
}
