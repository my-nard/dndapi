using System;

namespace dndHitpointApi.Models {
    // I'd rather avoid all the json strings on the server if I can
    public enum Statistics {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }


    // Convenience extensions for the JSON model for the creature/character stats
    public static class CreatureStatisticsExtensions {
        // Convenience method for getting the stat
        public static int StatisticValue(this CreatureStatistics creatureStatistics, Statistics stat) {
            switch (stat) {
                case Statistics.Strength: 
                    return creatureStatistics.Strength;

                case Statistics.Dexterity: 
                    return creatureStatistics.Dexterity;

                case Statistics.Constitution: 
                    return creatureStatistics.Constitution;

                case Statistics.Intelligence: 
                    return creatureStatistics.Intelligence;

                case Statistics.Wisdom: 
                    return creatureStatistics.Wisdom;

                case Statistics.Charisma: 
                    return creatureStatistics.Charisma;

                default:
                    return 0;
            }
        }

        // Gets the modifier value ((stat-10)/2, round down) for the given stat block and statistic
        public static int Modifier(this CreatureStatistics creatureStatistics, Statistics stat) {
            return determineStatModifier(creatureStatistics.StatisticValue(stat));
        }

        private static int determineStatModifier(int statValue) {
            return (int) Math.Floor((double) (statValue - 10) / 2);
        }

        public static CreatureStatistics Copy(this CreatureStatistics statistics) {
            return new CreatureStatistics() {
                Strength = statistics.Strength,
                Dexterity = statistics.Dexterity,
                Constitution = statistics.Constitution,
                Intelligence = statistics.Intelligence,
                Wisdom = statistics.Wisdom,
                Charisma = statistics.Charisma
            };
        }
    }
}