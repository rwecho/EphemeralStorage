
using System.Reflection;

namespace EphemeralStorage;

public static class PathUtilities 
{
    static PathUtilities()
    {
        var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!;

        AppData = Path.Combine(baseDirectory, "App_Data");
        Directory.CreateDirectory(AppData);

        Uploads = Path.Combine(AppData, "Uploads");
        Directory.CreateDirectory(Uploads);
    }
    public static string AppData { get; }

    public static string Uploads { get; }
}
