﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;
using RimWorld;


namespace LivingRim
{
    public static class CharacterContext
    {
        public static CharacterDetails GetCharacterDetails(string characterId, string persona, string description)
        {
            var pawn = Find.CurrentMap?.mapPawns?.AllPawns?.FirstOrDefault(p => p.ThingID.ToString() == characterId);
            if (pawn == null)
            {
                Log.Error($"Pawn with Character ID {characterId} not found.");
                return null;
            }

            var details = new CharacterDetails
            {
                characterId = characterId,
                name = pawn.Name?.ToStringShort ?? "Unknown",
                faction = pawn.Faction?.Name ?? "Unknown",
                gender = pawn.gender.ToString(),
                ageBiologicalYears = pawn.ageTracker?.AgeBiologicalYears.ToString() ?? "Unknown",
                ageChronologicalYears = pawn.ageTracker?.AgeChronologicalYears.ToString() ?? "Unknown",
                mood = pawn.needs?.mood?.CurLevel.ToString() ?? "Unknown",
                health = pawn.health?.summaryHealth?.SummaryHealthPercent.ToString() ?? "Unknown",
                hediffs = GetPawnHediffs(pawn),
                capacities = GetPawnCapacities(pawn),
                personality = GetPersonalityTraits(pawn),
                relationships = GetRelationships(pawn),
                environment = GetEnvironmentDetails(pawn),
                needs = GetAllNeeds(pawn),
                backstory = GetPawnBackstory(pawn),
                skills = GetPawnSkills(pawn),
                passions = GetPawnPassions(pawn),
                currentJob = pawn.CurJob?.def?.label ?? "None",
                inventory = GetPawnInventory(pawn),
                recentEvents = GetPawnRecentEvents(pawn),
                persona = persona,
                description = description,
                pronoun = GetPawnPronouns(pawn)
            };

            LogMissingFields(details);

            return details;
        }

        private static void LogMissingFields(CharacterDetails details)
        {
            foreach (var prop in details.GetType().GetProperties())
            {
                var value = prop.GetValue(details);
                if (value == null || value.ToString() == "Unknown")
                {
                    Log.Warning($"Character {details.characterId} - Missing or unknown field: {prop.Name}");
                }
            }
        }

        private static List<string> GetPawnHediffs(Pawn pawn)
        {
            return pawn.health?.hediffSet?.hediffs.Select(h => h.def.label).ToList() ?? new List<string>();
        }

        private static List<string> GetPawnCapacities(Pawn pawn)
        {
            var capacities = new List<string>();
            if (pawn.health != null && pawn.health.capacities != null)
            {
                foreach (var capacity in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
                {
                    float efficiency = pawn.health.capacities.GetLevel(capacity);
                    capacities.Add($"{capacity.label}: {efficiency.ToString("P0")}");
                }
            }
            return capacities;
        }

        private static List<string> GetPawnPronouns(Pawn pawn)
        {
            return new List<string> { pawn.gender.GetPronoun(), pawn.gender.GetPossessive(), pawn.gender.GetObjective() };
        }

        private static string GetPersonalityTraits(Pawn pawn)
        {
            return pawn != null ? string.Join(", ", pawn.story.traits.allTraits.Select(t => t.LabelCap)) : "None";
        }

        private static string GetRelationships(Pawn pawn)
        {
            return pawn != null ? string.Join(", ", pawn.relations.DirectRelations.Select(r => r.def.defName)) : "None";
        }

        private static Dictionary<string, int> GetPawnSkills(Pawn pawn)
        {
            return pawn.skills.skills.ToDictionary(skill => skill.def.defName, skill => skill.Level);
        }

        private static Dictionary<string, string> GetPawnPassions(Pawn pawn)
        {
            return pawn.skills.skills.ToDictionary(skill => skill.def.defName, skill => skill.passion.ToString());
        }

        private static string GetPawnInventory(Pawn pawn)
        {
            return pawn.inventory?.innerContainer?.ContentsString ?? "None";
        }

        private static string GetPawnBackstory(Pawn pawn)
        {
            var childhood = pawn.story.Childhood?.title ?? "Unknown Childhood";
            var adulthood = pawn.story.Adulthood?.title ?? "Unknown Adulthood";
            return $"Childhood: {childhood}, Adulthood: {adulthood}";
        }

        private static string GetEnvironmentDetails(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null)
            {
                return "Unknown";
            }

            var map = pawn.Map;
            var room = pawn.GetRoom(RegionType.Set_All);
            var temperature = map.mapTemperature.OutdoorTemp.ToString("F1");
            var weather = map.weatherManager.curWeather.label;
            var terrain = map.terrainGrid.TerrainAt(pawn.Position).label;
            var biome = map.Biome.label;
            var roomBeauty = room.GetStat(RoomStatDefOf.Impressiveness).ToString("F1");
            var timeOfDay = GenLocalDate.HourInteger(pawn.Map);

            return $"Temperature: {temperature}, Weather: {weather}, Terrain: {terrain}, Biome: {biome}, Room Beauty: {roomBeauty}, Time of Day: {timeOfDay}";
        }

        private static string GetAllNeeds(Pawn pawn)
        {
            if (pawn.needs == null)
                return "None";

            var needsList = pawn.needs.AllNeeds.Select(n => $"{n.LabelCap}: {n.CurLevelPercentage.ToString("P0")}");
            return string.Join(", ", needsList);
        }

        private static List<string> GetPawnRecentEvents(Pawn pawn)
        {
            return new List<string>(); // Add logic to fetch recent events if applicable
        }
    }
}
