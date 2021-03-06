﻿using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.OnePageDungeon
{
    public class DungeonAssetConfiguration
    {
        public float PositionDelta { get; set; } = 1;

        public IEnumerable<string> Volumes { get; set; } = new[] { "volume", "_volume", "_volume", "_volumes" };

        public IEnumerable<string> Floors { get; set; } = new string[] { };
        public IEnumerable<string> Walls { get; set; } = new string[] { };
        public IEnumerable<string> Ceilings { get; set; } = new string[] { };
        public IEnumerable<string> Columns { get; set; } = new string[] { };

        public IDictionary<DoorTypes, string[]> Doors { get; set; } = new Dictionary<DoorTypes, string[]> { };
        public IEnumerable<ModularSceneryObjectAnimationPlan> DoorAnimationPlans { get; set; }
        public IEnumerable<ModularSceneryObjectAction> DoorActions { get; set; }
        public IEnumerable<ModularSceneryObjectState> DoorStates { get; set; }

        public string GetRandonFloor()
        {
            return GetRandon(Floors);
        }

        public string GetRandonWall()
        {
            return GetRandon(Walls);
        }

        public string GetRandonCeiling()
        {
            return GetRandon(Ceilings);
        }

        public string GetRandonColumn()
        {
            return GetRandon(Columns);
        }

        public IEnumerable<string> GetDoor(DoorTypes doorType)
        {
            if (Doors.TryGetValue(doorType, out var res))
            {
                return res;
            }

            return new string[] { };
        }

        public T GetRandon<T>(IEnumerable<T> list)
        {
            if (list?.Any() != true)
            {
                return default;
            }

            int index = Helper.RandomGenerator.Next(0, list.Count());

            return list.ElementAt(index);
        }
    }
}
