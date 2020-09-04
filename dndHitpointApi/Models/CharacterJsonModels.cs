using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dndHitpointApi.Models {
    // "Root" model class for the Character JSON data
    // To keep things clearer I've got all the functions for things in extension classes in
    // separate files. Ultimately I'd probably build separate JSON-Model classes for each
    // version of the character JSON scheme we'd have and then map those to a full-blown
    // suite of classes, but this works for now.
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


    // Some of these could probably be enums but I'm not sure what the tests on your end are gonna be like.
    // I'd rather be relatively permissive with what I accept and just deal with the stringiness.
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
}