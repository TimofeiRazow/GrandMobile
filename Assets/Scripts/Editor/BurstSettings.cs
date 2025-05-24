using UnityEditor;
using Unity.Burst;

[InitializeOnLoad]
public class BurstSettings
{
    static BurstSettings()
    {
        // Disable Burst compilation for development
        BurstCompiler.Options.EnableBurstCompilation = false;
        BurstCompiler.Options.EnableBurstSafetyChecks = true;
    }
} 