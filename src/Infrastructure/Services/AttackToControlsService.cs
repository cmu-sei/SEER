// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services
{
    public class AttackToControlsService
    {
        private readonly AttackTechniquesRepository _attackTechniquesRepository;
        private readonly MitigationControlsRepository _mitigationControlsRepository;
        private readonly MapItemsRepository _mapItemsRepository;
        private readonly ILogger<AttackToControlsService> _logger;

        private readonly string FileStopWords = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "nist", "stop-words.txt");
        private readonly string FileAttackTechniques = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "nist", "nist800-53-r5-enterprise-attack.json");
        private readonly string FileMappings = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "nist", "nist800-53-r5-mappings.json");
        private readonly string FileMitigationControls = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "nist", "nist800-53-r5-controls.json");
        private readonly string[] StopWords;

        public AttackToControlsService(ILogger<AttackToControlsService> logger)
        {
            _logger = logger;

            var content = File.ReadAllText(this.FileAttackTechniques);
            this._attackTechniquesRepository = JsonConvert.DeserializeObject<AttackTechniquesRepository>(content);

            content = File.ReadAllText(this.FileMappings);
            this._mapItemsRepository = JsonConvert.DeserializeObject<MapItemsRepository>(content);

            content = File.ReadAllText(this.FileMitigationControls);
            this._mitigationControlsRepository = JsonConvert.DeserializeObject<MitigationControlsRepository>(content);

            var stopWordsFile = File.ReadAllText(this.FileStopWords);
            this.StopWords = stopWordsFile.Split(Convert.ToChar(Environment.NewLine));
        }

        public object GetMitigationControls(string query, int results = 10)
        {
            if (string.IsNullOrEmpty(query)) return new MitigationControls();

            var mitigationControls = this._mitigationControlsRepository.MitigationControls
                .Where(x => x.name != null
                            && !string.IsNullOrEmpty(x.name)
                            && x.name.ToLower()
                                .Contains(query.ToLower()))
                .Take(results).ToArray();

            foreach (var mitigationControl in mitigationControls)
            {
                var mappings = this._mapItemsRepository.Maps
                    .Where(x => x.source_ref == mitigationControl.id);
                foreach (var mapping in mappings)
                {
                    mitigationControl.AssociatedAttackTechniques = _attackTechniquesRepository.AttackTechniques.Where(x => x.id == mapping.target_ref);
                }
            }

            return new MitigationControls { Records = mitigationControls, Count = mitigationControls.Length };
        }

        public AttackTechniques GetAttackTechniques(string query, int results = 10)
        {
            if (string.IsNullOrEmpty(query)) return new AttackTechniques();

            var attackTechniques = this._attackTechniquesRepository.AttackTechniques
                .Where(x => x.name != null
                            && !string.IsNullOrEmpty(x.name)
                            && x.name.ToLower()
                                .Contains(query.ToLower()))
                .Take(results).ToArray();

            foreach (var attackTechnique in attackTechniques)
            {
                var mappings = this._mapItemsRepository.Maps
                    .Where(x => x.target_ref == attackTechnique.id);
                foreach (var mapping in mappings)
                {
                    attackTechnique.AssociatedMitigationControls = this._mitigationControlsRepository.MitigationControls
                        .Where(x => x.id == mapping.source_ref);
                }
            }

            return new AttackTechniques { Records = attackTechniques, Count = attackTechniques.Length };
        }

        public AttackTechniques Search(string query, int results = 20)
        {
            if (string.IsNullOrEmpty(query)) return new AttackTechniques();

            var attackTechniques = new List<AttackTechniquesRepository.AttackTechnique>();
            query = query.ToLower();

            var queries = query.Split(new[] { ' ', '.', '?' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var stopWord in this.StopWords)
            {
                queries.Remove(stopWord);
            }

            foreach (var q in queries)
            {
                attackTechniques.AddRange(this._attackTechniquesRepository.AttackTechniques
                    .Where(x => x.name != null
                                && !string.IsNullOrEmpty(x.name)
                                && x.name.ToLower()
                                    .Contains(q.ToLower()))
                    .Take(results).ToList());

                var mitigationControls = this._mitigationControlsRepository.MitigationControls
                    .Where(x => x.name != null
                                && !string.IsNullOrEmpty(x.name)
                                && x.name.ToLower()
                                    .Contains(q.ToLower()))
                    .Take(results).ToArray();

                foreach (var mitigationControl in mitigationControls)
                {
                    var mappings = this._mapItemsRepository.Maps
                        .Where(x => x.source_ref == mitigationControl.id);
                    foreach (var mapping in mappings)
                    {
                        attackTechniques.AddRange(_attackTechniquesRepository.AttackTechniques.Where(x => x.id == mapping.target_ref));
                    }
                }

                _logger.LogWarning($"{q} : {attackTechniques.Count}");
            }

            _logger.LogWarning(attackTechniques.Count.ToString());
            var duplicates = attackTechniques.GroupBy(x => x)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key).ToList();
            if (duplicates.Count > 0)
            {
                attackTechniques = duplicates;
            }

            attackTechniques = attackTechniques.Distinct().ToList();
            _logger.LogWarning(attackTechniques.Count.ToString());

            foreach (var attackTechnique in attackTechniques)
            {
                var mappings = this._mapItemsRepository.Maps
                    .Where(x => x.target_ref == attackTechnique.id);
                foreach (var mapping in mappings)
                {
                    attackTechnique.AssociatedMitigationControls = this._mitigationControlsRepository.MitigationControls
                        .Where(x => x.id == mapping.source_ref);
                }
            }

            return new AttackTechniques { Records = attackTechniques, Count = attackTechniques.Count };
        }
    }
}