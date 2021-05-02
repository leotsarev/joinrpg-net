namespace JoinRpg.Web.Models.Subscribe
{
    public class SubscribeListViewModel
    {
        public UserProfileDetailsViewModel User { get; set; }
        public SubscribeListItemViewModel[] Items { get; set; }

        public string[] PaymentTypeNames { get; set; }

        public bool AllowChanges { get; set; }

        public int ProjectId { get; set; }
    }
}
