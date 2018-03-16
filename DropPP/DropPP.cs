using System;
using System.Collections.Generic;
using System.IO;
using Fougerite;

namespace DropPP
{
    public class DropPP : Fougerite.Module
    {
        public override string Name
        {
            get { return "DropPP"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "DropPP"; }
        }

        public override Version Version
        {
            get { return new Version("1.0"); }
        }

        public override void Initialize()
        {
            Hooks.OnTablesLoaded += OnTablesLoaded;
        }

        public override void DeInitialize()
        {
            Hooks.OnTablesLoaded -= OnTablesLoaded;
        }

        public void OnTablesLoaded(Dictionary<string, LootSpawnList> Tables)
        {
            if (!Directory.Exists(ModuleFolder + "\\Tables"))
            {
                Directory.CreateDirectory(ModuleFolder + "\\Tables");
            }
            ExtractTables(Tables);
            
            foreach (var name in Tables.Keys)
            {				
                IniParser table = new IniParser(ModuleFolder + "\\Tables\\" + name + ".ini");
                LootSpawnList realTable = Tables[name];
                try
                {
                    realTable.minPackagesToSpawn = int.Parse(table.GetSetting("TableSettings", "MinToSpawn"));
                    realTable.maxPackagesToSpawn = int.Parse(table.GetSetting("TableSettings", "MaxToSpawn"));
                    realTable.spawnOneOfEach = bool.Parse(table.GetSetting("TableSettings", "OneOfEach"));
                    realTable.noDuplicates = bool.Parse(table.GetSetting("TableSettings", "DuplicatesAllowed"));
                    realTable.noDuplicates = !realTable.noDuplicates;
                }
                catch (Exception ex)
                {
                    Logger.LogError("[DropPP] Failed to convert values from the ini file! (0x1) " + ex);
                }

                var c = table.Count() - 1;
                var packs = new LootSpawnList.LootWeightedEntry[c];
			
                for (var i = 1; i <= c; i++)
                {
                    try
                    {
                        var pack = new LootSpawnList.LootWeightedEntry();
                        pack.weight = float.Parse(table.GetSetting("Entry" + i, "Weight"));
                        pack.amountMin = int.Parse(table.GetSetting("Entry" + i, "Min"));
                        pack.amountMax = int.Parse(table.GetSetting("Entry" + i, "Max"));

                        var objName = table.GetSetting("Entry" + i, "Name");

                        if (Tables.ContainsKey(objName))
                        {
                            pack.obj = Tables[objName];
                        }
                        else
                        {
                            pack.obj = Server.GetServer().Items.Find(objName);
                        }

                        packs[i - 1] = pack;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("[DropPP] Failed to convert values from the ini file! (0x2) " + ex);
                    }
                }
			
                realTable.LootPackages = packs;						
            }
        }
        
        public void ExtractTables(Dictionary<string, LootSpawnList> tbls)
        {	
            foreach (var name in tbls.Keys)
            {
                if (File.Exists(ModuleFolder + "\\Tables\\" + name))
                {
                    continue;
                }

                File.Create(ModuleFolder + "\\Tables\\" + name + ".ini").Dispose();
                IniParser table = new IniParser(ModuleFolder + "\\Tables\\" + name);
                table.AddSetting("TableSettings", "MinToSpawn", tbls[name].minPackagesToSpawn.ToString());
                table.AddSetting("TableSettings", "MaxToSpawn", tbls[name].maxPackagesToSpawn.ToString());
                table.AddSetting("TableSettings", "DuplicatesAllowed", (!tbls[name].noDuplicates).ToString());
                table.AddSetting("TableSettings", "OneOfEach", tbls[name].spawnOneOfEach.ToString());
				
                var cpt = 1;		
                foreach (var entry in tbls[name].LootPackages)
                {			
                    var n = "Entry" + cpt;
                    if(entry.obj != null)
                    {
                        table.AddSetting(n, "Name", entry.obj.name);
                        table.AddSetting(n, "Weight", entry.weight.ToString());
                        table.AddSetting(n, "Min", entry.amountMin.ToString());
                        table.AddSetting(n, "Max", entry.amountMax.ToString());
				
                        cpt++;
                    }
                }		
                table.Save();
            }	
        }
    }
}