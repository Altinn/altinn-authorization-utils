using Microsoft.Build.Evaluation;

namespace Altinn.Authorization.RepoCtl.Model.Utils;

internal static class MSBuildHelpers
{
    extension(Project project)
    {
        public bool GetPropertyValueAsBool(string propertyName)
        {
            var value = project.GetPropertyValue(propertyName);
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
