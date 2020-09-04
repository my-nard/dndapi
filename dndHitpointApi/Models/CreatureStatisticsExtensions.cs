using System;

namespace dndHitpointApi.Models {
    public enum Statistics {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }

    public static class CreatureStatisticsExtensions {
        public static int Modifier(this CreatureStatistics creatureStatistics, Statistics stat) {
            switch (stat) {
                case Statistics.Strength: 
                    return determineStatModifier(creatureStatistics.Strength);

                case Statistics.Dexterity: 
                    return determineStatModifier(creatureStatistics.Dexterity);

                case Statistics.Constitution: 
                    return determineStatModifier(creatureStatistics.Constitution);

                case Statistics.Intelligence: 
                    return determineStatModifier(creatureStatistics.Intelligence);

                case Statistics.Wisdom: 
                    return determineStatModifier(creatureStatistics.Wisdom);

                case Statistics.Charisma: 
                    return determineStatModifier(creatureStatistics.Charisma);

                default:
                    return 0;
            }
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