using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MESS.Application.UseCases.Tasks;

public static class TaskMetadataHelper
{
    public static (string CleanDescription, List<Guid> AssigneeIds) ParseDescription(string? rawDescription)
    {
        if (string.IsNullOrEmpty(rawDescription))
            return (string.Empty, new List<Guid>());

        var match = Regex.Match(rawDescription, @"<!--ASSIGNEES:(.*?)-->");
        if (!match.Success)
            return (rawDescription, new List<Guid>());

        var idsStr = match.Groups[1].Value;
        var ids = idsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                        .Where(g => g != Guid.Empty)
                        .ToList();

        var clean = rawDescription.Replace(match.Value, "").TrimEnd();
        return (clean, ids);
    }

    public static string FormatDescriptionWithAssignees(string? cleanDescription, List<Guid>? assigneeIds)
    {
        var baseDesc = cleanDescription ?? string.Empty;
        baseDesc = Regex.Replace(baseDesc, @"<!--ASSIGNEES:.*?-->", "").TrimEnd();

        if (assigneeIds != null && assigneeIds.Count > 0)
        {
            var uniqueIds = assigneeIds.Distinct().Where(id => id != Guid.Empty).ToList();
            if (uniqueIds.Count > 0)
            {
                return string.IsNullOrEmpty(baseDesc)
                    ? $"<!--ASSIGNEES:{string.Join(",", uniqueIds)}-->"
                    : $"{baseDesc}\n<!--ASSIGNEES:{string.Join(",", uniqueIds)}-->";
            }
        }

        return baseDesc;
    }
}
