using System.ComponentModel.DataAnnotations;
using JoinRpg.Web.Models.CommonTypes;

namespace JoinRpg.Web.Models.FieldSetup;

public class FieldSettingsViewModel : IFieldNavigationAware
{
    [Display(Name = "Имя персонажа привязано к", Description = "Привязывать можно только к полям персонажа типа строка.")]
    public int NameField { get; set; }
    [Display(Name = "Описание персонажа привязано к", Description = "Привязывать можно только к полям персонажа типа текст.")]
    public int DescriptionField { get; set; }

    public IReadOnlyCollection<JoinSelectListItem> PossibleNameFields { get; set; }
    public IReadOnlyCollection<JoinSelectListItem> PossibleDescriptionFields { get; set; }

    public FieldNavigationModel Navigation { get; private set; }

    public int ProjectId { get; set; }

    public void SetNavigation(FieldNavigationModel fieldNavigation)
    {
        Navigation = fieldNavigation;
        Navigation.Page = FieldNavigationPage.FieldSettings;
    }
}
