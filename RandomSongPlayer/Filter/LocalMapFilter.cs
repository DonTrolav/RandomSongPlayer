using SimpleJSON;
using SongDetailsCache.Structs;

namespace RandomSongPlayer.Filter
{
    internal class LocalMapFilter
    {
        private readonly System.Globalization.NumberStyles styleHex = System.Globalization.NumberStyles.HexNumber;
        private readonly System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

        private readonly bool minKeyEnabled;
        private readonly int minKey;
        private readonly bool maxKeyEnabled;
        private readonly int maxKey;

        private readonly bool minRatingEnabled;
        private readonly float minRating;
        private readonly bool maxRatingEnabled;
        private readonly float maxRating;

        private readonly bool minBPMEnabled;
        private readonly float minBPM;
        private readonly bool maxBPMEnabled;
        private readonly float maxBPM;

        private readonly bool minDurationEnabled;
        private readonly uint minDuration;
        private readonly bool maxDurationEnabled;
        private readonly uint maxDuration;

        private readonly bool minVotesEnabled;
        private readonly uint minVotes;
        private readonly bool maxVotesEnabled;
        private readonly uint maxVotes;

        private readonly bool minUpvotesEnabled;
        private readonly uint minUpvotes;
        private readonly bool maxUpvotesEnabled;
        private readonly uint maxUpvotes;

        private readonly bool minDownvotesEnabled;
        private readonly uint minDownvotes;
        private readonly bool maxDownvotesEnabled;
        private readonly uint maxDownvotes;

        private readonly bool minDownloadsEnabled;
        private readonly uint minDownloads;
        private readonly bool maxDownloadsEnabled;
        private readonly uint maxDownloads;

        private readonly bool minUPDEnabled;
        private readonly float minUPD;
        private readonly bool maxUPDEnabled;
        private readonly float maxUPD;

        internal LocalMapFilter(JSONNode filterSet)
        {
            minKeyEnabled = filterSet["mapMinKey"] != null && int.TryParse(filterSet["mapMinKey"], styleHex, provider, out minKey);
            maxKeyEnabled = filterSet["mapMaxKey"] != null && int.TryParse(filterSet["mapMaxKey"], styleHex, provider, out maxKey);
            minRatingEnabled = filterSet["mapMinRating"] != null && float.TryParse(filterSet["mapMinRating"], out minRating);
            maxRatingEnabled = filterSet["mapMaxRating"] != null && float.TryParse(filterSet["mapMaxRating"], out maxRating);
            minBPMEnabled = filterSet["mapMinBPM"] != null && float.TryParse(filterSet["mapMinBPM"], out minBPM);
            maxBPMEnabled = filterSet["mapMaxBPM"] != null && float.TryParse(filterSet["mapMaxBPM"], out maxBPM);
            minDurationEnabled = filterSet["mapMinDuration"] != null && uint.TryParse(filterSet["mapMinDuration"], out minDuration);
            maxDurationEnabled = filterSet["mapMaxDuration"] != null && uint.TryParse(filterSet["mapMaxDuration"], out maxDuration);
            minVotesEnabled = filterSet["mapMinVotes"] != null && uint.TryParse(filterSet["mapMinVotes"], out minVotes);
            maxVotesEnabled = filterSet["mapMaxVotes"] != null && uint.TryParse(filterSet["mapMaxVotes"], out maxVotes);
            minUpvotesEnabled = filterSet["mapMinUpvotes"] != null && uint.TryParse(filterSet["mapMinUpvotes"], out minUpvotes);
            maxUpvotesEnabled = filterSet["mapMaxUpvotes"] != null && uint.TryParse(filterSet["mapMaxUpvotes"], out maxUpvotes);
            minDownvotesEnabled = filterSet["mapMinDownvotes"] != null && uint.TryParse(filterSet["mapMinDownvotes"], out minDownvotes);
            maxDownvotesEnabled = filterSet["mapMaxDownvotes"] != null && uint.TryParse(filterSet["mapMaxDownvotes"], out maxDownvotes);
            minDownloadsEnabled = filterSet["mapMinDownloads"] != null && uint.TryParse(filterSet["mapMinDownloads"], out minDownloads);
            maxDownloadsEnabled = filterSet["mapMaxDownloads"] != null && uint.TryParse(filterSet["mapMaxDownloads"], out maxDownloads);
            minUPDEnabled = filterSet["mapMinUDP"] != null && float.TryParse(filterSet["mapMinUDP"], out minUPD);
            maxUPDEnabled = filterSet["mapMaxUDP"] != null && float.TryParse(filterSet["mapMaxUDP"], out maxUPD);
        }

        internal bool CheckFilter(Song song)
        {
            int key = int.Parse(song.key, styleHex);
            if (minKeyEnabled && key < minKey) return false;
            if (maxKeyEnabled && key > maxKey) return false;
            if (minRatingEnabled && (song.rating == 0 ? 0.5 : song.rating) < minRating) return false;
            if (maxRatingEnabled && (song.rating == 0 ? 0.5 : song.rating) > maxRating) return false;
            if (minBPMEnabled && song.bpm < minBPM) return false;
            if (maxBPMEnabled && song.bpm > maxBPM) return false;
            if (minDurationEnabled && song.songDurationSeconds < minDuration) return false;
            if (maxDurationEnabled && song.songDurationSeconds > maxDuration) return false;
            if (minVotesEnabled && song.upvotes + song.downvotes < minVotes) return false;
            if (maxVotesEnabled && song.upvotes + song.downvotes > maxVotes) return false;
            if (minUpvotesEnabled && song.upvotes < minUpvotes) return false;
            if (maxUpvotesEnabled && song.upvotes > maxUpvotes) return false;
            if (minDownvotesEnabled && song.downvotes < minDownvotes) return false;
            if (maxDownvotesEnabled && song.downvotes > maxDownvotes) return false;
            if (minDownloadsEnabled && song.downloadCount < minDownloads) return false;
            if (maxDownloadsEnabled && song.downloadCount > maxDownloads) return false;
            if (minUPDEnabled && (song.downloadCount == 0 || song.upvotes < minUPD * song.downloadCount)) return false;
            if (maxUPDEnabled && (song.downloadCount == 0 || song.upvotes > maxUPD * song.downloadCount)) return false;
            return true;
        }
    }
}
