using JoinRpg.DataModel;
using JoinRpg.PrimitiveTypes.ProjectMetadata;

namespace JoinRpg.Domain.Schedules;

/// <summary>
/// Builds schedule from program item data
/// </summary>
public class ScheduleBuilder
{
    private readonly ICollection<Character> characters;
    private readonly ProjectInfo projectInfo;

    private ProjectField TimeSlotField { get; }

    private ProjectField RoomField { get; }

    public ScheduleBuilder(Project project, ICollection<Character> characters, ProjectInfo projectInfo)
    {
        this.characters = characters.Where(ch => ch.IsActive).ToList();
        RoomField = project.GetRoomFieldOrDefault() ?? throw new Exception("Schedule not enabled");
        TimeSlotField = project.GetTimeSlotFieldOrDefault() ?? throw new Exception("Schedule not enabled");
        this.projectInfo = projectInfo;
    }

    private HashSet<ProgramItem> NotScheduled { get; } = new HashSet<ProgramItem>();

    public class ProgramItemSlot
    {
        public ProgramItemSlot(TimeSlot slot, ScheduleRoom room)
        {
            Room = room;
            TimeSlot = slot;
        }
        public ScheduleRoom Room { get; }
        public TimeSlot TimeSlot { get; }
        public ProgramItem? ProgramItem { get; set; } = null;
    }

    private List<List<ProgramItemSlot>> Slots { get; set; } = null!; // Initialized in start of Build()
    private List<ScheduleRoom> Rooms { get; set; } = null!; // Initialized in start of Build()

    private List<TimeSlot> TimeSlots { get; set; } = null!; // Initialized in start of Build()

    private HashSet<ProgramItem> Conflicted { get; } = new HashSet<ProgramItem>();

    public ScheduleResult Build()
    {
        Rooms = InitializeScheduleRoomList(RoomField.GetOrderedValues()).ToList();
        TimeSlots = InitializeTimeSlotList(TimeSlotField.GetOrderedValues()).ToList();
        Slots = InitializeSlots(TimeSlots, Rooms);

        var allItems = new List<ProgramItemPlaced>();

        foreach (var character in characters.Where(ch => ch.CharacterType != PrimitiveTypes.CharacterType.Slot))
        {
            var programItem = new ProgramItem(character);
            var slots = SelectSlots(programItem, character);
            PutItem(programItem, slots);
            if (slots.Any())
            {
                allItems.Add(new ProgramItemPlaced(programItem, slots));
            }
        }
        return new ScheduleResult(
            NotScheduled.ToList(),
            Rooms,
            TimeSlots,
            Conflicted.ToList(),
            Slots.Select(row => row.Select(r => r.ProgramItem).ToList()).ToList(),
            allItems);
    }

    private void PutItem(ProgramItem programItem, List<ProgramItemSlot> slots)
    {
        if (!slots.Any())
        {
            _ = NotScheduled.Add(programItem);
            return;
        }

        var conflicts = (from slot in slots
                         where slot.ProgramItem != null
                         select slot.ProgramItem).Distinct().ToList();

        if (conflicts.Any())
        {
            _ = Conflicted.Add(programItem);
            foreach (var conflict in conflicts)
            {
                _ = Conflicted.Add(conflict);
            }
        }
        else
        {
            foreach (var slot in slots)
            {
                slot.ProgramItem = programItem;
            }
        }
    }

    private List<ProgramItemSlot> SelectSlots(ProgramItem programItem, Character character)
    {
        var fields = character.GetFields(projectInfo);

        List<int> GetSlotIndexes(ProjectField field, IEnumerable<ScheduleItemAttribute> items)
        {
            var variantIds = fields
                .Single(f => f.Field.ProjectFieldId == field.ProjectFieldId)
                .GetDropdownValues()
                .Select(variant => variant.ProjectFieldDropdownValueId)
                .ToList();
            var indexes = (from item in items where variantIds.Contains(item.Id) select item.SeqId).ToList();
            if (indexes.Count < variantIds.Count) // Some variants not found, probably deleted
            {
                _ = NotScheduled.Add(programItem);
            }

            return indexes;
        }

        var slots = from timeSeqId in GetSlotIndexes(TimeSlotField, TimeSlots)
                    from roomSeqId in GetSlotIndexes(RoomField, Rooms)
                    select Slots[timeSeqId][roomSeqId];

        return slots.ToList();
    }

    private static List<List<ProgramItemSlot>> InitializeSlots(List<TimeSlot> timeSlots, List<ScheduleRoom> rooms)
        => timeSlots.Select(time => rooms.Select(room => new ProgramItemSlot(time, room)).ToList()).ToList();

    private static IEnumerable<ScheduleRoom> InitializeScheduleRoomList(IReadOnlyList<ProjectFieldDropdownValue> readOnlyList)
    {
        var seqId = 0;
        foreach (var variant in readOnlyList.Where(x => x.IsActive))
        {
            var item = new ScheduleRoom(variant, seqId);
            yield return item;
            seqId++;
        }
    }

    private static IEnumerable<TimeSlot> InitializeTimeSlotList(IReadOnlyList<ProjectFieldDropdownValue> readOnlyList)
    {
        var seqId = 0;
        foreach (var variant in readOnlyList.Where(x => x.IsActive))
        {
            var item = new TimeSlot(variant, seqId);
            yield return item;
            seqId++;
        }
    }
}
