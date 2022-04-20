using System.Collections.Generic;

namespace CotpsBot.Models.Http
{
    public class TeamInfoData
    {
        public List<TeamMember>? list { get; set; }
        public TeamDetail? detail { get; set; }
    }
}