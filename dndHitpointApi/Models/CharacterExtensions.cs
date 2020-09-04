using System.Collections.Generic;
using System;
using System.Linq;

namespace dndHitpointApi.Models {
    public static class CharacterExtensions {
        // Rather than apply the max hitpoints to the character, figure out the hitpoints for them
        // Let the caller choose whether to apply or not.
        public static CreatureHitPoints CalculateMaximumHitpoints(this Character character) {
            // Assumption: this is going to work like D&D5E. That is: the first level
            // of the first class is your starting class and you get the full hit die
            // worth of HP
            CreatureHitPoints hitPoints = new CreatureHitPoints();

            if (character.Classes == null || character.Classes.Count == 0) {
                return hitPoints;
            }
            
            // Make an ordered list of the char's class hit dice (since ordering probably matters, at least for the first one)
            List<int> classHitDice = makeListOfCharClassHitDieValues(character.Classes);

            // Work off of the derived statistics rather than the model statistics
            // Need to account for items/blessings/etc.
            int extraHpPerLevel = character.DerivedStatistics().Modifier(Statistics.Constitution);
            //and if (character.Race == "hilldwarf") { extraHpPerLevel += 1; }

            hitPoints.Maximum = calculateMaxHitPointsUsingDieAverage(character, classHitDice, extraHpPerLevel);

            return hitPoints;
        }

        // Return an ordered list of hit dice
        // I figured it'd be nice to return a die for every level; that way we can preserve some ordering.
        private static List<int> makeListOfCharClassHitDieValues(IList<CharacterClassDetails> classes) {
            var classHitDice = new List<int>();

            foreach (CharacterClassDetails classDetails in classes) {
                for (int level = 0; level < classDetails.ClassLevel; level++) {
                    classHitDice.Add(classDetails.HitDiceValue);
                }
            }

            return classHitDice;
        }

        // decided to just implement the die average HP calc
        // we'd probably want to update the model to represent each level for random rolls
        private static int calculateMaxHitPointsUsingDieAverage(Character character, List<int> classHitDice, int hitPointsPerLevel) {
            int hitPoints = 0;

            if (classHitDice.Count == 0) {
                return 0;
            }

            // The first level's HP = the hit die
            int curLevelHitDie = classHitDice[0];
            // I suppose level 1 can be a killer.
            if (curLevelHitDie > 0) {
                hitPoints += (curLevelHitDie + hitPointsPerLevel);
            }

            for (int level = 1; level < classHitDice.Count; level++) {
                curLevelHitDie = classHitDice[level];

                if (curLevelHitDie > 0) {
                    // die sides / 2 + .5 => average, round up.
                    // would technically apply to odd sided dice, too
                    hitPoints += (int) System.Math.Ceiling(curLevelHitDie / 2 + .5);
                    hitPoints += hitPointsPerLevel;
                }
            }

            return hitPoints;
        }


        // Rather than storing the item-adjusted statistics on the model
        // I've chosen to calculate them when I need them - it's not computationally hard
        // and it keeps me from having to ensure two data structures are accurate.
        public static CreatureStatistics DerivedStatistics(this Character character) {
            CreatureStatistics creatureStatistics = character.Stats.Copy();
            
            // Theoretically we could apply the adjustments right to the stats here, but
            // it seemed like it might be nice to resolve all of the item bonuses first
            // If we ever do stuff like giant belts we'll want to be aware of the order we
            // do things in (eg my Str is set to 21, but another str bonus item shouldn't raise it past 20)
            CreatureStatistics statModifiers = determineStatisticAdjustments(character);

            creatureStatistics.Strength += statModifiers.Strength;
            creatureStatistics.Dexterity += statModifiers.Dexterity;
            creatureStatistics.Constitution += statModifiers.Constitution;
            creatureStatistics.Intelligence += statModifiers.Intelligence;
            creatureStatistics.Wisdom += statModifiers.Wisdom;
            creatureStatistics.Charisma += statModifiers.Charisma;

            return creatureStatistics;
        }

        // Returns a stat array that represents modifiers to the char's existing stats
        // If I wanted to model stuff like the giant belts I'd probably split that mechanism out
        // into a separate function
        private static CreatureStatistics determineStatisticAdjustments(Character character) {
            CreatureStatistics statModifiers = new CreatureStatistics();

            // Looks like the only thing we've got in the model that affects this right now is items
            foreach (CharacterItem item in character.Items) {
                applyItemModifierToStats(item, statModifiers);
            }

            return statModifiers;
        }

        // Takes an items, looks at its modifier object, and adjusts the statistics accordingly
        private static void applyItemModifierToStats(CharacterItem item, CreatureStatistics statistics) {
            if (item == null) return;
            if (item.Modifier == null) return;

            // If we're not affecting the stats array, dont bother.
            if (!CharacterJsonConstants.ObjectTypeStats.Equals(item.Modifier.AffectedObject?.ToLower())) {
                return;
            }

            switch (item.Modifier.AffectedValue?.ToLower()) {
                case CharacterJsonConstants.StatStrength:
                    statistics.Strength += item.Modifier.Value;
                    break;

                case CharacterJsonConstants.StatDexterity:
                    statistics.Dexterity += item.Modifier.Value;
                    break;

                case CharacterJsonConstants.StatConstitution:
                    statistics.Constitution += item.Modifier.Value;
                    break;

                case CharacterJsonConstants.StatIntelligence:
                    statistics.Intelligence += item.Modifier.Value;
                    break;

                case CharacterJsonConstants.StatWisdom:
                    statistics.Wisdom += item.Modifier.Value;
                    break;

                case CharacterJsonConstants.StatCharisma:
                    statistics.Charisma += item.Modifier.Value;
                    break;

                default:
                    break;
            }
        }

        // Applies healing to a character.
        // Returns true if the HP are changed
        public static bool ApplyHealing(this Character character, int healAmount) {
            if (character == null) return false;
            if (character.HitPoints == null) return false;
            if (healAmount < 1) return false;

            // Don't bother saving hitpoints if the current=max -> save the DB write
            if (character.HitPoints.Current == character.HitPoints.Maximum) return false;

            // Don't heal past the maximum
            character.HitPoints.Current = Math.Min(character.HitPoints.Current + healAmount, character.HitPoints.Maximum);

            return true;
        }

        // Gives a character temp HP
        // Returns true if the HP are changed
        public static bool GrantTemporaryHitpoints(this Character character, int tempHitpointAmount) {
            if (character == null) return false;
            if (character.HitPoints == null) return false;
            if (tempHitpointAmount < 1) return false;

            // one final guard - don't apply the HP if they're the same to avoid a db write
            if (character.HitPoints.Temporary == tempHitpointAmount) return false;

            // If they've got temp HP already, use the max of the two values
            character.HitPoints.Temporary = Math.Max(character.HitPoints.Temporary, tempHitpointAmount);

            return true;
        }


        // convenience enumeration 
        public enum DamageResistance {
            None = 0,
            Resistant = 1,
            Immune = 2            
        }

        public static bool ApplyDamage(this Character character, DealtDamageInformation damageInformation) {
            if (character == null) return false;
            if (character.HitPoints == null) return false;
            if (damageInformation == null) return false;
            if (damageInformation.Value < 0) return false;
            if (String.IsNullOrEmpty(damageInformation.Type)) return false;

            DamageResistance typeResistance = character.damageResistanceForDamageInfo(damageInformation);

            int damageRemaining = damageValueForResistance(damageInformation.Value, typeResistance);

            if (character.HitPoints.Temporary > 0) {
                if (character.HitPoints.Temporary > damageRemaining) {
                    character.HitPoints.Temporary -= damageRemaining;
                    damageRemaining = 0;
                }
                else {
                    damageRemaining -= character.HitPoints.Temporary;
                    character.HitPoints.Temporary = 0;
                }
            }

            if ((damageRemaining > 0) && (character.HitPoints.Current > 0)) {
                if (damageRemaining > character.HitPoints.Current) {
                    damageRemaining = damageRemaining - character.HitPoints.Current;
                    character.HitPoints.Current = 0;
                }
                else {
                    character.HitPoints.Current -= damageRemaining;
                    damageRemaining = 0;
                }
            }

            // damageRemaining might be useful for determining whether this char got one-shot
            // But I'm not planning to implement that for this.

            return true;
        }

        // Figure out what sort of resistance the character has for the given damage type
        private static DamageResistance damageResistanceForDamageInfo(this Character character, DealtDamageInformation damageInformation) {
            Dictionary<string, DamageResistance> resistances = character.DamageResistances();
            string type = damageInformation.Type.ToLower();

            if (resistances.ContainsKey(type)) {
                return resistances[type];
            }
            
            return DamageResistance.None;
        }

        // Determine how much damage to apply to the character given their resistance and the original damage
        private static int damageValueForResistance(int originalDamageValue, DamageResistance resistance) {
            switch (resistance) {
                case DamageResistance.Immune:
                    return 0;

                case DamageResistance.Resistant:
                    return originalDamageValue / 2; // implicit round-down

                default:
                    return originalDamageValue;
            }
        }

        // Converts between the JSON string for the resistance type and our enum
        private static DamageResistance damageResistanceForValue(string damageTypeValue) {
            switch (damageTypeValue.ToLower()) {
                case CharacterJsonConstants.DefenseResistance:
                    return DamageResistance.Resistant;

                case CharacterJsonConstants.DefenseImmunity:
                    return DamageResistance.Immune;

                default:
                    return DamageResistance.None;
            }
        }




        
        public static Dictionary<string, DamageResistance> DamageResistances(this Character character) {
            var resistances = new Dictionary<string, DamageResistance>();
            
            foreach (CreatureDefenseDetails defenseDetails in character.Defenses) {
                tryAddDefenseToResistanceMap(defenseDetails, resistances);
            }

            return resistances;
        }

        // Adds a given set of defenses to a map between the damage type title and our damage resistance enum
        // Checks to make sure that we're only using the most restrictive defense.
        // That is, if someone's resistant AND immune, they're immune.
        private static bool tryAddDefenseToResistanceMap(CreatureDefenseDetails defenseDetails, Dictionary<string, DamageResistance> resistances) {
            if (defenseDetails == null) return false;
            if (resistances == null) return false;

            if (String.IsNullOrEmpty(defenseDetails.Defense)) return false;
            if (String.IsNullOrEmpty(defenseDetails.Type)) return false;

            string type = defenseDetails.Type.ToLower();
            DamageResistance currentResistance = damageResistanceForValue(defenseDetails.Defense);
            
            if (resistances.ContainsKey(type)) {
                DamageResistance resistance = resistances[type];
                
                // Immune > Resistant > None
                if (resistance > currentResistance) {
                    return false;
                }

                resistances.Remove(type);
            }
            
            resistances.Add(type, currentResistance);
            return true;
        }
    }
}