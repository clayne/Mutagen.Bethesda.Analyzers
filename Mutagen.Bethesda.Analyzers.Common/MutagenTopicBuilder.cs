﻿using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Mutagen.Bethesda.Analyzers
{
    public static class MutagenTopicBuilder
    {
        public static TopicDefinition FromDiscussion(
            ushort id,
            string title,
            Severity severity)
        {
            return TopicDefinition.FromDiscussion(id, title, severity, Constants.AnalyzersDiscussionUrl);
        }
    }
}