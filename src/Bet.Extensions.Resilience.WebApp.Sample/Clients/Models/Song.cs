using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.WebApp.Sample.Clients.Models
{
    public class Song
    {
        public Song()
        {
            Tags = new List<string>();
            Genres = new List<string>();
        }

        public string Name { get; set; }

        public string HebrewName { get; set; }

        public int Number { get; set; }

        public string Album { get; set; }

        public string Artist { get; set; }

        public Uri AlbumArtUri { get; set; }

        public string PurchaseUri { get; set; }

        public Uri Uri { get; set; }

        public LikeStatus SongLike { get; set; }

        public int CommunityRank { get; set; }

        public CommunityRankStanding CommunityRankStanding { get; set; }

        public string Id { get; set; }

        public List<string> Tags { get; set; }

        public List<string> Genres { get; set; }

        public string Lyrics { get; set; }

        public int TotalPlays { get; set; }

        public string AlbumId { get; set; }

        public string ArtistId { get; set; }

        public int CommentCount { get; set; }

        public AlbumColors AlbumColors { get; set; }

        public SongPickReasons ReasonsPlayed { get; set; }
    }
}
