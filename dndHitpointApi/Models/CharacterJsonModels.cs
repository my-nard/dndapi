using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dndHitpointApi.Models {
    public class CharacterJsonConstants {
        public const string ObjectTypeStats = "stats";

        public const string DefenseResistance = "resistance";
        public const string DefenseImmunity = "immunity";

        public const string StatStrength = "strength";
        public const string StatDexterity = "dexterity";
        public const string StatConstitution = "constitution";
        public const string StatIntelligence = "intelligence";
        public const string StatWisdom = "wisdom";
        public const string StatCharisma = "charisma";
        
    }
    

    public class Character {
        public String Id { get; set; }
        public String Name { get; set; }
        public int Level { get; set; }

        public IList<CharacterClassDetails> Classes { get; set; }

        public CreatureStatistics Stats { get; set; }

        public IList<CharacterItem> Items { get; set; }

        public IList<CreatureDefenseDetails> Defenses { get; set; }

        public CreatureHitPoints HitPoints { get; set; }
    }

    public class CreatureHitPoints {
        public int Maximum { get; set; }
        public int Current { get; set; }
        public int Temporary { get; set; }
    }

    public class CharacterClassDetails {
        public String Name { get; set; }
        public int HitDiceValue { get; set; }
        public int ClassLevel { get; set; }
    }

    public class CreatureStatistics {
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
    }


    public class CharacterItem {
        public class ItemModifier {
            public string AffectedObject { get; set; }
            public string AffectedValue { get; set; }
            public int Value { get; set; }
        }

        public string Name { get; set; }
        public ItemModifier Modifier { get; set; }
    }

    public class CreatureDefenseDetails {
        public String Type { get; set; }
        public String Defense { get; set; }
    }

    public class DealtDamageInformation {
        public int Value { get; set; }
        public string Type { get; set; }
    }


    public class ErrorInformation {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}