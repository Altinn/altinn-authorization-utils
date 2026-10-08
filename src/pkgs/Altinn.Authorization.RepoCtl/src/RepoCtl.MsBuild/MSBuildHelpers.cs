namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

/// <summary>
/// Provides helper methods for working with MSBuild projects.
/// </summary>
public static class MSBuildHelpers
{
    extension(IMsBuildProjectSnapshot project)
    {
        /// <summary>
        /// Gets the value of the specified property as a boolean. Returns false if the property is not set or is empty.
        /// </summary>
        /// <param name="propertyName">The property name to retrieve the value for.</param>
        /// <returns>True if the property value is "true" (case-insensitive), otherwise false.</returns>
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
