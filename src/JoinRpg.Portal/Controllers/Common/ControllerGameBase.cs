using JetBrains.Annotations;
using JoinRpg.Data.Interfaces;
using JoinRpg.DataModel;
using JoinRpg.Portal.Infrastructure.DiscoverFilters;
using JoinRpg.Services.Interfaces.Projects;
using JoinRpg.Web.Filter;
using JoinRpg.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace JoinRpg.Portal.Controllers.Common;

[CaptureNoAccessExceptionHandler]
[DiscoverProjectFilter]
public abstract class ControllerGameBase : LegacyJoinControllerBase
{
    [ProvidesContext, NotNull]
    protected IProjectService ProjectService { get; }
    public IProjectRepository ProjectRepository { get; }

    protected ControllerGameBase(
        IProjectRepository projectRepository,
        IProjectService projectService,
        IUserRepository userRepository
        ) : base(userRepository)
    {
        ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        ProjectService = projectService;
    }

    protected ActionResult NoAccesToProjectView(Project project) => View("ErrorNoAccessToProject", new ErrorNoAccessToProjectViewModel(project));

    [Obsolete]
    protected async Task<Project> GetProjectFromList(int projectId, IEnumerable<IProjectEntity> folders) => folders.FirstOrDefault()?.Project ?? await ProjectRepository.GetProjectAsync(projectId);


    protected ActionResult RedirectToIndex(Project project) => RedirectToAction("Index", "GameGroups", new { project.ProjectId, area = "" });

    protected ActionResult RedirectToIndex(int projectId, int characterGroupId, [AspMvcAction] string action = "Index") => RedirectToAction(action, "GameGroups", new { projectId, characterGroupId, area = "" });

    protected async Task<ActionResult> RedirectToProject(int projectId)
    {
        var project = await ProjectRepository.GetProjectAsync(projectId);
        return project == null ? NotFound() : RedirectToIndex(project);
    }
}
