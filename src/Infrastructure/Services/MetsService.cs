using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services;

public class MetsService
{
    private static Logger _log = LogManager.GetCurrentClassLogger();
    
    private readonly ApplicationDbContext _db;

    public MetsService(ApplicationDbContext dbContext)
    {
        this._db = dbContext;
    }

    public class Item
    {
        public string Name { get; set; }
        public IEnumerable<string> Scts { get; set; }
    }

    public class Jmet
    {
        public string Name { get; set; }
        public IEnumerable<Item> Items { get; set; }
    }

    public class Met
    {
        public string Name { get; set; }
        public IEnumerable<string> Scts { get; set; }
    }

    public class MetlConfiguration
    {
        public IEnumerable<Met> Mets { get; set; }
        public IEnumerable<Jmet> Jmets { get; set; }
    }

    public MetlConfiguration ReadConfiguration(string path)
    {
        MetlConfiguration o = null;
        try
        {
            var metlConfig = File.ReadAllText(path);
            o = JsonConvert.DeserializeObject<MetlConfiguration>(metlConfig);
        }
        catch (Exception ex)
        {
            _log.Error($"Metl configuration could not be loaded: {ex}");
        }

        return o;
    }

    public string GetPath()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "config", $"metl.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        return File.Exists(path) ? path : Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "config", "metl.json");
    }

    /// <summary>
    /// Build new METL from configuration
    /// </summary>
    public async Task CreateMetl(int? assessmentId)
    {
        //load file from local config
        var path = GetPath();
        if (File.Exists(path))
        {
            var metlConfiguration = ReadConfiguration(path);

            try
            {
                var metIndex = 1;
                foreach (var met in metlConfiguration.Mets)
                {
                    // mets have no item, so for every met, there is a item which we stuff with that met's scts
                    var m = new MET
                    {
                        Name = met.Name, AssessmentId = assessmentId.Value, Type = MetType.MET,
                        Created = DateTime.UtcNow
                    };
                    var metItem = new METItem { Index = metIndex, Name = met.Name, Created = DateTime.UtcNow };
                    var sctIndex = 1;
                    foreach (var sct in met.Scts)
                    {
                        metItem.METSCTs.Add(new METItemSCT
                        {
                            Index = sctIndex, Name = sct, Status = ActiveStatus.Active, Title = "Step",
                            Created = DateTime.UtcNow
                        });
                        sctIndex++;
                    }

                    m.METItems.Add(metItem);
                    _db.METs.Add(m);
                    metIndex++;
                }

                metIndex = 1;
                foreach (var jmet in metlConfiguration.Jmets)
                {
                    var m = new MET
                    {
                        Name = jmet.Name, AssessmentId = assessmentId.Value, Type = MetType.JMET,
                        Created = DateTime.UtcNow
                    };
                    foreach (var item in jmet.Items)
                    {
                        var sctIndex = 1;
                        var i = new METItem { Name = item.Name, Index = metIndex, Created = DateTime.UtcNow };
                        if (item.Scts != null)
                        {
                            foreach (var sct in item.Scts)
                            {
                                i.METSCTs.Add(new METItemSCT
                                {
                                    Index = sctIndex, Name = sct, Status = ActiveStatus.Active, Title = "Step",
                                    Created = DateTime.UtcNow
                                });
                                sctIndex++;
                            }
                        }

                        m.METItems.Add(i);
                    }

                    _db.METs.Add(m);
                    metIndex++;
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}