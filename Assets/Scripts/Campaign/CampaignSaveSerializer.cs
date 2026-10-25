using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Campaign
{
    public sealed class CampaignSaveFormatException : FormatException
    {
        public CampaignSaveFormatException(string reason)
            : base("Invalid campaign save: " + reason)
        {
        }

        public CampaignSaveFormatException(string reason, Exception inner)
            : base("Invalid campaign save: " + reason, inner)
        {
        }
    }

    public static class CampaignSaveSerializer
    {
        public const int CurrentVersion = 1;

        public static string Serialize(CampaignProgress progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            // Region ids are only 'a'..'z' (Region contract), so no JSON escaping is needed.
            var sb = new StringBuilder();
            sb.Append("{\"version\":").Append(CurrentVersion).Append(",\"liberated\":[");
            IReadOnlyList<string> ids = progress.LiberatedRegionIds;
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append('"').Append(ids[i]).Append('"');
            }

            sb.Append("]}");
            return sb.ToString();
        }

        public static CampaignProgress Deserialize(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            object root = JsonReader.Parse(json);
            var members = root as List<KeyValuePair<string, object>>;
            if (members == null)
            {
                throw new CampaignSaveFormatException("the root must be an object");
            }

            bool hasVersion = false;
            object version = null;
            bool hasLiberated = false;
            object liberated = null;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, object> member in members)
            {
                if (!seen.Add(member.Key))
                {
                    throw new CampaignSaveFormatException("duplicate key '" + member.Key + "'");
                }

                if (member.Key == "version")
                {
                    hasVersion = true;
                    version = member.Value;
                }
                else if (member.Key == "liberated")
                {
                    hasLiberated = true;
                    liberated = member.Value;
                }
            }

            if (!hasVersion)
            {
                throw new CampaignSaveFormatException("missing 'version'");
            }

            if (!(version is long) || (long)version != CurrentVersion)
            {
                throw new CampaignSaveFormatException("'version' must be the integer " + CurrentVersion);
            }

            if (!hasLiberated)
            {
                throw new CampaignSaveFormatException("missing 'liberated'");
            }

            var array = liberated as List<object>;
            if (array == null)
            {
                throw new CampaignSaveFormatException("'liberated' must be an array");
            }

            var ids = new List<string>(array.Count);
            foreach (object item in array)
            {
                var id = item as string;
                if (id == null)
                {
                    throw new CampaignSaveFormatException("'liberated' must contain only strings");
                }

                ids.Add(id);
            }

            try
            {
                return CampaignProgress.FromLiberated(ids);
            }
            catch (ArgumentException ex)
            {
                throw new CampaignSaveFormatException("invalid 'liberated' list: " + ex.Message, ex);
            }
        }
    }
}
