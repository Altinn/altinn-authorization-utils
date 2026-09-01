namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

internal static class MSBuildHelpers
{
    extension(IMsBuildProjectSnapshot project)
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
