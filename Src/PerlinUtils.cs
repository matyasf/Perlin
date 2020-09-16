using System.IO;
using System.Reflection;

namespace Perlin
{
    internal static class PerlinUtils
    {

        private const string ResourcePrefix = "Perlin.Src.Assets.";

        internal static byte[] LoadEmbeddedResourceAsByte(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            //var allResources = assembly.GetManifestResourceNames();
            using Stream stream = assembly.GetManifestResourceStream(ResourcePrefix + resourceName);
            using StreamReader reader = new StreamReader(stream);
            using var memStream = new MemoryStream();
            reader.BaseStream.CopyTo(memStream);
            return memStream.ToArray();
        }

        internal static Stream LoadEmbeddedResourceAsString(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + resourceName);
        }
    }
}
