namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using System.IO;
    using TPRandomizer.FcSettings.Enums;

    /// <summary>
    /// summary text.
    /// </summary>
    public class SoundAssets
    {
        private class bgmReplacement
        {
            public int originalBgmTrack { get; set; }
            public int replacementBgmTrack { get; set; }
            public int replacementBgmWave { get; set; }
        };

        private class bgmData
        {
            public string name { get; set; }
            public string internalName { get; set; }
            public int bgmID { get; set; }
            public int bgmWave { get; set; }
            public bool sceneBgm { get; set; }
            public bool dungeonBgm { get; set; }
            public bool minibossBgm { get; set; }
            public bool bossBgm { get; set; }
            public bool minigameBgm { get; set; }
            public bool eventBgm { get; set; }
            public bool cutsceneBgm { get; set; }
            public bool isFanfare { get; set; }
        };

        //The Bgm Section will be laid out as follows: (BgmReplacementCount,
        //{bgmId,replacementId,replacementWave},{...},fanfareReplacementCount,fanfareId,replacementId,{...})
        public static List<byte> GenerateBgmData(SeedData seedData)
        {
            List<byte> data = new();
            List<bgmData> replacementPool = new();
            List<bgmReplacement> bgmReplacementArray = new();
            if (seedData.fcSettings.randomizeBgm == RandomizeBgm.Off)
            {
                return data;
            }
            Dictionary<string, bgmData> dataList = JsonConvert.DeserializeObject<
                Dictionary<string, bgmData>
            >(File.ReadAllText(Global.CombineRootPath("./Assets/Sound/BackgroundMusic.json")));
            if (seedData.fcSettings.randomizeBgm != RandomizeBgm.Off)
            {
                foreach (KeyValuePair<string, bgmData> currentData in dataList)
                {
                    if (
                        seedData.fcSettings.randomizeBgm == RandomizeBgm.Overworld
                        && currentData.Value.sceneBgm == true
                    )
                    {
                        replacementPool.Add(currentData.Value);
                    }
                    if (
                        seedData.fcSettings.randomizeBgm == RandomizeBgm.Dungeon
                        && currentData.Value.dungeonBgm == true
                    )
                    {
                        replacementPool.Add(currentData.Value);
                    }
                    if (
                        seedData.fcSettings.randomizeBgm == RandomizeBgm.All
                        && (
                            currentData.Value.sceneBgm == true
                            || currentData.Value.bossBgm == true
                            || currentData.Value.minibossBgm == true
                            || currentData.Value.minigameBgm == true
                            || currentData.Value.eventBgm == true
                        )
                    )
                    {
                        replacementPool.Add(currentData.Value);
                    }
                }
                foreach (bgmData currentData in replacementPool)
                {
                    bgmReplacement replacement = new();
                    replacement.replacementBgmTrack = currentData.bgmID;
                    replacement.replacementBgmWave = currentData.bgmWave;
                    Random rnd = new();
                    while (true)
                    {
                        replacement.originalBgmTrack = replacementPool[
                            rnd.Next(replacementPool.Count)
                        ].bgmID;
                        bool foundSame = false;
                        foreach (bgmReplacement currentReplacement in bgmReplacementArray)
                        {
                            if (currentReplacement.originalBgmTrack == replacement.originalBgmTrack)
                            {
                                foundSame = true;
                                break;
                            }
                        }
                        if (foundSame == false)
                        {
                            break;
                        }
                    }
                    bgmReplacementArray.Add(replacement);
                }
                if (replacementPool.Count != bgmReplacementArray.Count)
                {
                    Console.WriteLine(
                        "BGM Pool ("
                            + replacementPool.Count
                            + ") and Replacement ("
                            + bgmReplacementArray.Count
                            + ") have different lengths!"
                    );
                }
                foreach (bgmReplacement currentReplacement in bgmReplacementArray)
                {
                    data.Add((byte)currentReplacement.originalBgmTrack);
                    data.Add((byte)currentReplacement.replacementBgmTrack);
                    data.Add((byte)currentReplacement.replacementBgmWave);
                    data.Add((byte)0x0); // Padding
                }
            }
            seedData.BgmHeaderRaw.bgmTableNumEntries = ((byte)bgmReplacementArray.Count);
            seedData.BgmHeaderRaw.bgmTableSize = (UInt16)data.Count;
            return data;
        }

        public static List<byte> GenerateFanfareData(SeedData seedData)
        {
            List<byte> data = new();
            List<bgmData> replacementPool = new();
            List<bgmReplacement> fanfareReplacementArray = new();
            if (seedData.fcSettings.randomizeFanfares)
            {
                Dictionary<string, bgmData> dataList = JsonConvert.DeserializeObject<
                    Dictionary<string, bgmData>
                >(File.ReadAllText("./Generator/Assets/Sound/BackgroundMusic.json"));
                foreach (KeyValuePair<string, bgmData> currentData in dataList)
                {
                    if (currentData.Value.isFanfare == true && currentData.Value.bgmWave == 0)
                    {
                        replacementPool.Add(currentData.Value);
                    }
                }
                foreach (bgmData currentData in replacementPool)
                {
                    bgmReplacement replacement = new();
                    replacement.replacementBgmTrack = currentData.bgmID;
                    replacement.replacementBgmWave = 0;
                    Random rnd = new();
                    while (true)
                    {
                        replacement.originalBgmTrack = replacementPool[
                            rnd.Next(replacementPool.Count)
                        ].bgmID;
                        bool foundSame = false;
                        foreach (bgmReplacement currentReplacement in fanfareReplacementArray)
                        {
                            if (currentReplacement.originalBgmTrack == replacement.originalBgmTrack)
                            {
                                foundSame = true;
                                break;
                            }
                        }
                        if (foundSame == false)
                        {
                            break;
                        }
                    }
                    fanfareReplacementArray.Add(replacement);
                }
                if (replacementPool.Count != fanfareReplacementArray.Count)
                {
                    Console.WriteLine(
                        "Fanfare Pool ("
                            + replacementPool.Count
                            + ") and Replacement ("
                            + fanfareReplacementArray.Count
                            + ") have different lengths!"
                    );
                }
                foreach (bgmReplacement currentReplacement in fanfareReplacementArray)
                {
                    data.Add((byte)currentReplacement.originalBgmTrack);
                    data.Add((byte)currentReplacement.replacementBgmTrack);
                    data.Add((byte)0x0); // Padding
                    data.Add((byte)0x0); // Padding
                }
            }

            seedData.BgmHeaderRaw.fanfareTableNumEntries = (byte)fanfareReplacementArray.Count;
            seedData.BgmHeaderRaw.fanfareTableSize = (UInt16)data.Count;

            return data;
        }
    }
}
