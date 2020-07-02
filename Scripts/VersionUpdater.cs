#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class VersionUpdater : MonoBehaviour
{
    [Serializable]
    public enum VT
    {
        DEV,
        PLAYTEST,
        ALPHA,
        BETA
    }
    public VT VersionType;

    void Start()
    {
        BuildInfo.AddCount(BuildInfo.tcount.RunCount);
    }
}

public class BuildVerUpdate : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        BuildInfo.AddCount(BuildInfo.tcount.BuildCount);
    }
}

public class BuildInfo
{
    public enum tcount
    {
        MainVer,
        SubVer,
        BuildCount,
        RunCount
    }
    public string VersionType = "DEV"; 
    public string MainVer = "0";
    public string SubVer = "0";
    public string BuildCount = "00000";
    public string RunCount = "00000";

    static string BuildPath = Application.dataPath.ToString() + "/BuildInfo/BuildInfo.json";
    static string DataPath = Application.dataPath.ToString().TrimEnd("Assets".ToCharArray()) + "ProjectSettings/ProjectSettings.asset";

#region Update

    public static void AddCount(tcount t)
    {
        BuildInfo currentBI = ReadBuildInfo();
        switch (t)
        {
            case tcount.MainVer:
                int mv = int.Parse(currentBI.MainVer);
                currentBI.MainVer = (mv + 1).ToString().PadLeft(1, '0');
                break;
            case tcount.SubVer:
                int sv = int.Parse(currentBI.SubVer);
                currentBI.SubVer = (sv + 1).ToString().PadLeft(1, '0');
                break;
            case tcount.BuildCount:
                int bc = int.Parse(currentBI.BuildCount);
                currentBI.BuildCount = (bc + 1).ToString().PadLeft(6, '0');
                break;
            case tcount.RunCount:
                int rc = int.Parse(currentBI.RunCount);
                currentBI.RunCount = (rc + 1).ToString().PadLeft(6, '0');
                break;
        }
        Debug.Log("Build Count Updated!");
        SaveNewVersion(currentBI);
        Debug.Log($"Version: {currentBI.VersionType}.{currentBI.MainVer}.{currentBI.SubVer}.{currentBI.BuildCount}.{currentBI.RunCount}");
    }
#endregion

#region Read/Write
    static BuildInfo ReadBuildInfo()//Read the previous version from ProjectFolder/Assets/BuildInfo/BuildInfo.json
    {
        if (!File.Exists(BuildPath)) SaveNewVersion(new BuildInfo());
        BuildInfo bi = JsonConvert.DeserializeObject<BuildInfo>(File.ReadAllText(BuildPath));
        return bi;
    }

    static void SaveNewVersion(BuildInfo bi)
    {
        //Save BuildInfo.json
        File.WriteAllText(BuildPath, JsonConvert.SerializeObject(bi));

        //Save ProjectSettings.asset
        string[] data = ReadProjectSettings();
        string vertxt = data.Single(xa => xa.Contains("  bundleVersion: "));
        int ind = Array.IndexOf(data, vertxt);
        data[ind] = $"  bundleVersion: {bi.VersionType}.{bi.MainVer}.{bi.SubVer}.{bi.BuildCount}.{bi.RunCount}";
        SaveProjectSettings(data);
    }

    static string[] ReadProjectSettings()//Read the current version from ProjectFolder/ProjectSettings/ProjectSettings.asset
    {
        string[] filecontent = File.ReadAllLines(DataPath);
        return filecontent;
    }

    static void SaveProjectSettings(string[] ary)//Overwrite the file at ProjectFolder/ProjectSettings/ProjectSettings.asset
    {
        File.WriteAllLines(DataPath, ary);
    }
    #endregion
}
#endif
