using JoinRpg.DataModel;
using JoinRpg.TestHelpers;
using Shouldly;
using Xunit;

namespace JoinRpg.Domain.Test
{
    /// <summary>
    /// This class basically tests that we don't forget to update some places when adding new project field types
    /// </summary>
    public class ProjectFieldTypeTests
    {
        [Theory]
        [MemberData(nameof(FieldTypes))]
        public void PricingDecided(ProjectFieldType projectFieldType)
        {
            Should.NotThrow(() => projectFieldType.SupportsPricing());
        }

        [Theory]
        [MemberData(nameof(FieldTypes))]
        public void PricingOnFieldDecided(ProjectFieldType projectFieldType)
        {
            Should.NotThrow(() => projectFieldType.SupportsPricingOnField());
        }

        [Theory]
        [MemberData(nameof(FieldTypes))]
        public void HasValuesListDecided(ProjectFieldType projectFieldType)
        {
            Should.NotThrow(() => projectFieldType.HasValuesList());
        }

        [Theory]
        [MemberData(nameof(FieldTypes))]
        public void CanHaveValueDecided(ProjectFieldType projectFieldType)
        {
            Should.NotThrow(() => projectFieldType.CanHaveValue());
        }


        [Theory]
        [MemberData(nameof(FieldTypes))]
        public void HasValuesListDecided(ProjectFieldType projectFieldType)
        {
            Should.NotThrow(() => projectFieldType.HasValuesList());
        }

        [Theory]
        [MemberData(nameof(FieldTypes))]
        public void PriceDecided(ProjectFieldType projectFieldType)
        {
            //This is crude test, not mean to test actual finance logic
            var fieldWithValue = new FieldWithValue(new ProjectField() {FieldType = projectFieldType}, "0");
            Should.NotThrow(() =>fieldWithValue.GetCurrentFee());
        }

        // ReSharper disable once MemberCanBePrivate.Global xUnit requirements
        public static TheoryData<ProjectFieldType> FieldTypes =>
            EnumerationTestHelper.GetTheoryDataForAllEnumValues<ProjectFieldType>();
    }
}
