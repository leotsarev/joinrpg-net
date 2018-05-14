using JoinRpg.DataModel;
using Shouldly;

namespace JoinRpg.Domain.Test
{
    internal static class ShouldyDataModelExtensions
    {
        public static void FieldValuesShouldBe(this IFieldContainter mockCharacter,
            params FieldWithValue[] field2) =>
            mockCharacter.JsonData.ShouldBe(field2.SerializeFields());
    }
}
