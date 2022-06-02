﻿using System;
using System.Collections.Generic;

namespace ASC.Files.Core
{
    public interface ITagDao : IDisposable
    {
        IEnumerable<Tag> GetTags(Guid owner, TagType tagType);

        IEnumerable<Tag> GetTags(string name, TagType tagType);

        IEnumerable<Tag> GetTags(string[] names, TagType tagType);

        IEnumerable<Tag> SaveTags(params Tag[] tag);

        void RemoveTags(params Tag[] tag);

        void RemoveTags(params int[] tagIds);
    }
}
