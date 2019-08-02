using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Diagnostics;

const string ModName = "ItemSpawn";
const string pathToGameDir = @"D:\SteamLibrary\steamapps\common\H3VR";
readonly string[] deps = new[] { "Valve.Newtonsoft.Json.dll"};

var gameDir = new DirectoryInfo(pathToGameDir);
var kolebriExe = gameDir.GetFiles("Kolibri.Inject.exe").First();
if (!kolebriExe.Exists)
{
    Console.WriteLine($"Coulnt find kolibri in {pathToGameDir}");
    return;
}

var modsDir = gameDir.GetDirectories("Mods").FirstOrDefault();
if (modsDir == null)
    modsDir = Directory.CreateDirectory(Path.Combine(gameDir.FullName, "Mods"));

var modDir = modsDir.GetDirectories(ModName).FirstOrDefault();
if (modDir == null)
    modDir = Directory.CreateDirectory(Path.Combine(modsDir.FullName, ModName));

var buildDir = new DirectoryInfo(Directory.GetCurrentDirectory());
var mainModFile = new FileInfo(Path.Combine(buildDir.FullName, $"{ModName}.dll"));

var copiedModFile = new FileInfo(Path.Combine(modDir.FullName, mainModFile.Name));
File.Copy(mainModFile.FullName, copiedModFile.FullName, true);

var configFile = new FileInfo(Path.Combine(copiedModFile.Directory.FullName, "ItemSpawnerConfig.json"));
if (!configFile.Exists)
{
    var mainConfigFile = new FileInfo(Path.Combine(buildDir.FullName, $"ItemSpawnerConfig.json"));
    File.Copy(mainConfigFile.FullName, configFile.FullName);
}

if (deps.Any())
{
    var depDir = new DirectoryInfo(Path.Combine(copiedModFile.Directory.FullName, "Dependencies"));
    if (!depDir.Exists)
        Directory.CreateDirectory(depDir.FullName);
    foreach(var dep in deps)
    {
        File.Copy(Path.Combine(Directory.GetCurrentDirectory(), dep), Path.Combine(depDir.FullName, dep), true);
    }
}

var kolibriProcess = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = kolebriExe.FullName,
        WorkingDirectory = kolebriExe.Directory.FullName
    }
};

kolibriProcess.Start();
kolibriProcess.WaitForExit();