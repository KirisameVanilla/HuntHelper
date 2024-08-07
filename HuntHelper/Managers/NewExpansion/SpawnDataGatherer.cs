﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace HuntHelper.Managers.NewExpansion
{
    internal static class SpawnDataGatherer
    {
        //&name= mobid=  &map= &mapid= &rank= &playerid= &x= &y= &z=
        private static readonly string baseUrl = @"https://idklol-cqej.onrender.com/api/dawntrail?"; // first 2 maps have S/SS disabled atm. still want data
        private static IList<MobFoundData> history = new List<MobFoundData>();

        public static void AddFoundMob(uint mobid, string name, Vector3 position, string rank, uint mapid, string mapName, ulong playerid)
        {
            if (!Constants.NEW_EXPANSION) return;

            var currTime = DateTime.UtcNow;

            ClearOldMobs(currTime);
            SubmitToApi(mobid, name, position, mapid, mapName, rank, currTime, playerid);
        }

        private static async void SubmitToApi(uint mobid, string name, Vector3 position, uint mapid, string mapName, string rank, DateTime currTime, ulong playerid)
        {
            if (InRecentHistory(mobid)) return;
            try
            {
                var foundMob = new MobFoundData(mobid, currTime);
                history.Add(foundMob);
                var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"{playerid}")));
                // y = z, z = y. but i already swapped them. so y = y, z = z, z conversion is not correct btw
                var url = baseUrl + $"map={mapName}&mapid={mapid}&mobid={mobid}&name={name}&rank={rank}&playerid={hash}&x={position.X}&y={position.Y}&z={position.Z}";



                using HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 10, 0);
                var res = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
#if DEBUG
                PluginLog.Debug($"Trying {url}");
                PluginLog.Error($"{res.StatusCode} : {playerid}  : {hash}");

#endif
                if (!res.IsSuccessStatusCode) history.Remove(foundMob);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"{ex.Message}");
                PluginLog.Warning($"Could not submit data: {mobid}:{name}:{position}:{mapid}");
            }
        }

        private static bool InRecentHistory(uint mobid)
        {
            if (history.Any(m => m.Id == mobid)) return true;
            return false;

        }

        private static void ClearOldMobs(DateTime timeToCompare)
        {

            var newHistory = new List<MobFoundData>();
            foreach (var item in history)
            {   //if mob less than 30 seconds old, keep in history.
                if (timeToCompare.Subtract(item.Date).TotalMinutes < 0.5)
                    newHistory.Add(item);
            }
            history = newHistory;
        }

        private class MobFoundData
        {
            public uint Id { get; init; }
            public DateTime Date { get; init; }

            public MobFoundData(uint id, DateTime date)
            {
                Id = id;
                Date = date;
            }
        }
    }


}
