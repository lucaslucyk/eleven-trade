namespace CotpsBot.Models
{
    public class SubscriptionPlan
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Interval { get; set; }
        public bool Active { get; set; } = true;
    }
}