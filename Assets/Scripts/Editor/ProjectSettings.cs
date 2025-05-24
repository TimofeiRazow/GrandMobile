using UnityEditor;
using UnityEngine;

public class ProjectSettings : EditorWindow
{
    [MenuItem("Tools/Configure Project Settings")]
    public static void ConfigureProjectSettings()
    {
        // Configure Scripting Backend
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        
        // Configure API Compatibility Level
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
        
        // Configure Assembly Definition References
        var assemblyDefinition = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>("Assets/Scripts/Assembly-CSharp.asmdef");
        if (assemblyDefinition == null)
        {
            Debug.Log("Creating Assembly Definition...");
            var assemblyDefinitionJson = @"{
                ""name"": ""Assembly-CSharp"",
                ""rootNamespace"": """",
                ""references"": [],
                ""includePlatforms"": [],
                ""excludePlatforms"": [],
                ""allowUnsafeCode"": false,
                ""overrideReferences"": false,
                ""precompiledReferences"": [],
                ""autoReferenced"": true,
                ""defineConstraints"": [],
                ""versionDefines"": [],
                ""noEngineReferences"": false
            }";
            System.IO.File.WriteAllText("Assets/Scripts/Assembly-CSharp.asmdef", assemblyDefinitionJson);
            AssetDatabase.Refresh();
        }
        
        Debug.Log("Project settings configured successfully!");
    }
} 