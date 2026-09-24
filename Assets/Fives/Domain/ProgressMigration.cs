using System.Collections.Generic;
using System.Linq;

namespace Fives.Domain
{
    /// <summary>
    /// Save format versions. Version 0 stored themes and puzzles by display name, so renaming or translating
    /// content silently reset progress; version 1 stores stable ids.
    /// </summary>
    public static class ProgressMigration
    {
        public const int CurrentVersion = 1;

        /// <summary>
        /// Maps version 0 names to ids. A name maps to every id that carried it; entries matching no known name,
        /// such as removed content, are kept unchanged.
        /// </summary>
        public static List<string> NamesToIds(IEnumerable<string> entries, IEnumerable<(string Name, string Id)> catalog)
        {
            var ids = catalog.ToLookup(item => item.Name, item => item.Id);
            return entries
                .Where(entry => !string.IsNullOrEmpty(entry))
                .SelectMany(entry => ids.Contains(entry) ? ids[entry] : new[] { entry })
                .Distinct()
                .ToList();
        }
    }
}
