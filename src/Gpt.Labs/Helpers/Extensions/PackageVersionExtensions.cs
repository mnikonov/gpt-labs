using Windows.ApplicationModel;

namespace Gpt.Labs.Helpers.Extensions
{
    public static class PackageVersionExtensions
    {
        public static string GetStringVersion(this PackageVersion version)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
