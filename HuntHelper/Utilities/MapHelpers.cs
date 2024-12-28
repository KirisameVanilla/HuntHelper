using Dalamud.Game;
using Dalamud.Plugin.Services;
using HuntHelper.Managers.Hunts.Models;
using Lumina.Data;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;

namespace HuntHelper.Utilities;
/// <summary>
/// Rename this class... DataManagerUtil? ExcelUtil? idk
/// </summary>
public class MapHelpers
{

    public static IDataManager DataManager;

    public static void SetUp(IDataManager dataManager) => DataManager = dataManager;

    public static string GetZoneName(uint territoryID) => DataManager.Excel.GetSheet<TerritoryType>().GetRowOrDefault(territoryID)?.PlaceName.ValueNullable?.Name.ExtractText() ?? "location not found";
    //map id... territory id... confusing ...

    public static uint GetMapID(uint territoryID) => DataManager.GetExcelSheet<TerritoryType>().GetRowOrDefault(territoryID)?.Map.RowId ?? 0;
    //createmaplink doesn't work with "Mor Dhona" :(

    //convert map scale (100/95) to map size (41/43.1)
    public static float MapScaleToMaxCoord(float mapScale) => (-0.42f) * mapScale + 83f;

    public static float ConvertToMapCoordinate(float pos, float mapScale) => 2048f / mapScale + pos / 50f + 1f;

    public static Vector2 ConvertToMapCoordinate(Vector3 pos, float mapScale) => new (
        ConvertToMapCoordinate(pos.X, mapScale),
        ConvertToMapCoordinate(pos.Z, mapScale)
    );

    public static void LocaliseMobNames(List<HuntTrainMob> trainList) => trainList.ForEach(m => m.Name = DataManager.Excel.GetSheet<BNpcName>()?.GetRowOrDefault(m.MobID)?.Singular.ToString() ?? m.Name);


    public static async Task<bool> MapImageVerUpToDate(string currentVersion)
    {
        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            var ver = await client.GetStringAsync(ImageVerUrl);
            PluginLog.Warning($"map images latest ver: {ver} Local ver: {currentVersion}");
            return currentVersion == ver;
        }
        catch (Exception ex)
        {
            PluginLog.Warning("Could not check map image version");
            PluginLog.Error(ex.Message);
        }
        return true;
    }

    public static async Task<string> GetMapImageVer()
    {

        try
        {
            var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("User-Agent", "request");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            var ver = await client.GetStringAsync(ImageVerUrl);
            return ver;
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex.Message);
        }
        return "0";
    }

#if DEBUG
    private static readonly string ImageVerUrl = @"https://raw.githubusercontent.com/img02/HuntHelper-Resources/test/version";
#else
    private static readonly string ImageVerUrl = @"https://raw.githubusercontent.com/img02/HuntHelper-Resources/main/version";
#endif
}