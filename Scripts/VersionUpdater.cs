using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
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

    string BuildPath;
    string DataPath;

#if UNITY_EDITOR
    private void Awake()
    {
        BuildPath = Application.dataPath.ToString() + "/BuildInfo/BuildInfo.json";
        DataPath = Application.dataPath.ToString().TrimEnd("Assets".ToCharArray()) + "ProjectSettings/ProjectSettings.asset";
    }

    void Start()
    {
        AddRunCount();
        SaveVersionToEditor();
    }

    private void AddRunCount()
    {
        BuildInfo currentBI = ReadBuildData();
        int rc = int.Parse(currentBI.RunCount);
        currentBI.RunCount = (rc + 1).ToString().PadLeft(6, '0');
        Debug.Log("Run Count Updated!");
        SaveBuildData(currentBI);
        Debug.Log($"Version: {currentBI.VersionType}.{currentBI.MainVer}.{currentBI.SubVar}.{currentBI.BuildVar}.{currentBI.RunCount}");
    }

    private void SaveVersionToEditor()
    {
        BuildInfo currentBI = ReadBuildData();
        string[] data = ReadVersionData();
        string vertxt = data.Single(xa => xa.Contains("  bundleVersion: "));
        int ind = Array.IndexOf(data, vertxt);
        data[ind] = $"  bundleVersion: {currentBI.VersionType}.{currentBI.MainVer}.{currentBI.SubVar}.{currentBI.BuildVar}.{currentBI.RunCount}";
        SaveVersionData(data);
    }

    private BuildInfo ReadBuildData()
    {
        if (!File.Exists(BuildPath)) SaveBuildData(new BuildInfo());
        BuildInfo bi = JsonConvert.DeserializeObject<BuildInfo>(File.ReadAllText(BuildPath));
        return bi;
    }
    private void SaveBuildData(BuildInfo bi)
    {
        File.WriteAllText(BuildPath, JsonConvert.SerializeObject(bi));
        AssetDatabase.Refresh();
    }
    private string[] ReadVersionData()
    {
        string[] filecontent = File.ReadAllLines(DataPath);
        return filecontent;
    }
    private void SaveVersionData(string[] ary)
    {
        File.WriteAllLines(DataPath, ary);
        AssetDatabase.Refresh();
    }
#endif
}

public class BuildInfo
{
    public string VersionType = "DEV"; 
    public int MainVer = 0;
    public int SubVar = 0;
    public string BuildVar = "00000";
    public string RunCount = "00000";
}
