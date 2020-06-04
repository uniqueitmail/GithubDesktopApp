﻿using System.Collections.Generic;

namespace GitHub.Core.Dtos
{
    public class RepositoriesRootObject
    {
        public int total_count { get; set; }
        public bool incomplete_results { get; set; }
        public List<RepositoriesFromApi> Items { get; set; }
    }
}