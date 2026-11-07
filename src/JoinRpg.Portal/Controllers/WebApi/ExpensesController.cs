using JoinRpg.Data.Interfaces;
using JoinRpg.Domain;
using JoinRpg.DomainTypes.Finances;
using JoinRpg.Interfaces;
using JoinRpg.Portal.Infrastructure.Authorization;
using JoinRpg.Services.Interfaces;
using JoinRpg.Web.Finances.Expenses;
using Microsoft.AspNetCore.Mvc;

namespace JoinRpg.Portal.Controllers.WebApi;

/// <summary>
/// Расходы проекта (ADR030). Смотреть может любой мастер; создавать — любой мастер;
/// править — мастер свои расходы или созданные им (проверяет сервис); одобрять/отклонять —
/// только с <see cref="Permission.CanManageMoney"/>.
/// </summary>
[Route("/webapi/{projectId}/expenses/[action]")]
[RequireMaster]
[ApiController]
public class ExpensesController(
    IExpenseRepository expenseRepository,
    IExpenseService expenseService,
    ICurrentUserAccessor currentUserAccessor,
    IProjectMetadataRepository projectMetadataRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseViewModel>>> List(ProjectIdentification projectId)
    {
        var projectInfo = await projectMetadataRepository.GetProjectMetadata(projectId);
        return Ok(ToDtos(projectInfo, await expenseRepository.GetExpenses(projectId)));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseViewModel>>> My(ProjectIdentification projectId)
    {
        var projectInfo = await projectMetadataRepository.GetProjectMetadata(projectId);
        return Ok(ToDtos(projectInfo, await expenseRepository.GetExpensesByMaster(projectId, currentUserAccessor.UserIdentification)));
    }

    /// <summary>Может ли текущий мастер одобрять и отклонять расходы — для кнопок на странице.</summary>
    [HttpGet]
    public async Task<ActionResult<bool>> CanManageMoney(ProjectIdentification projectId)
    {
        var projectInfo = await projectMetadataRepository.GetProjectMetadata(projectId);
        return Ok(projectInfo.HasMasterAccess(currentUserAccessor, Permission.CanManageMoney));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseIdentification>> Create(ProjectIdentification projectId, ExpenseEditViewModel model)
    {
        var id = await expenseService.CreateExpense(projectId, new CreateExpenseRequest(
            model.MasterId!,
            model.Name,
            model.CategoryPath,
            model.PlannedQuantity,
            model.PlannedPrice,
            ToMarkdown(model.Description)));
        return Ok(id);
    }

    // Расход склеивается с projectId, по которому проверены права, и числом из query — как в соседних ручках.
    [HttpPost]
    public async Task<ActionResult> Edit(ProjectIdentification projectId, int expenseId, ExpenseEditViewModel model)
    {
        await expenseService.EditExpense(new(projectId, expenseId), new EditExpenseRequest(
            model.MasterId!,
            model.Name,
            model.CategoryPath,
            model.PlannedQuantity,
            model.PlannedPrice,
            ToMarkdown(model.Description)));
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> SetActual(ProjectIdentification projectId, int expenseId, ExpenseActualRequest model)
    {
        await expenseService.SetExpenseActual(new(projectId, expenseId), model);
        return Ok();
    }

    [HttpPost]
    [RequireMaster(Permission.CanManageMoney)]
    public async Task<ActionResult> Approve(ProjectIdentification projectId, int expenseId, ExpenseCommentRequest? comment)
    {
        await expenseService.ApproveExpense(new(projectId, expenseId), ToMarkdown(comment?.Comment));
        return Ok();
    }

    [HttpPost]
    [RequireMaster(Permission.CanManageMoney)]
    public async Task<ActionResult> Reject(ProjectIdentification projectId, int expenseId, ExpenseCommentRequest? comment)
    {
        await expenseService.RejectExpense(new(projectId, expenseId), ToMarkdown(comment?.Comment));
        return Ok();
    }

    private static MarkdownString? ToMarkdown(string? markdown)
        => string.IsNullOrWhiteSpace(markdown) ? null : new MarkdownString(markdown);

    private static ExpenseViewModel ToDto(ExpenseInfo expense, Dictionary<UserIdentification, string> masterNames)
        => new(
            expense.ExpenseId,
            expense.Name,
            string.Join("\\", expense.CategoryPath.Select(segment => segment.Name)),
            expense.PlannedQuantity,
            expense.PlannedPrice,
            expense.ActualQuantity,
            expense.ActualPrice,
            expense.SpentAt,
            expense.State.ToView(),
            masterNames[expense.MasterId],
            masterNames[expense.CreatedById],
            expense.Description?.Value,
            expense.MasterId);

    private static ExpenseViewModel[] ToDtos(ProjectInfo projectInfo, IReadOnlyCollection<ExpenseInfo> expenses)
    {
        // Имена — из метаданных проекта: UserInfoHeader есть у каждого мастера, включая бывших.
        var masterNames = projectInfo.Masters
            .Concat(projectInfo.FormerMasters)
            .ToDictionary(master => master.UserId, master => master.UserInfo.DisplayName.DisplayName);
        return expenses.Select(expense => ToDto(expense, masterNames)).ToArray();
    }
}

/// <summary>Опциональный комментарий к решению по расходу (markdown).</summary>
public record ExpenseCommentRequest(string? Comment);
