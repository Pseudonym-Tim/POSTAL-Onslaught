#if UNITY_EDITOR
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

/// <summary>
/// Handles preprocessing operations before building the project...
/// </summary>
public class BuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        // Make sure the item database is refreshed before building!
        ItemDatabase.RefreshDatabase();
    }
}
#endif