namespace Bet.Extensions.Resilience.WebApp.Sample.Clients.Models
{
    public class SongPickReasons
    {
        /// <summary>
        /// Used when the user requests a particular song, artist, album, or tag.
        /// </summary>
        public SongPick? SoleReason { get; set; }
    }
}
