using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using dndHitpointApi.Models;


namespace dndHitpointApi.Database {

    // "You can store the data however you'd like"
    // Big trouble in little bigtable!
    // Thought about building out entity relationships for Character, but I figured it'd be overkill for the exercise
    public class CharacterSchema {
        public String Id { get; set; }
        public String CharacterJsonRepresentation { get; set; }
    }
    
    public class CharacterDbContext : DbContext {
        public CharacterDbContext(DbContextOptions<CharacterDbContext> options) : base(options) {
        }

        public DbSet<CharacterSchema> Characters {
            get;
            set;
        }
        public async Task<Character> GetCharacter(string id) {
            CharacterSchema charJson = await Characters.FindAsync(id);

            if (charJson == null) return null;
            if (String.IsNullOrEmpty(charJson.CharacterJsonRepresentation)) return null;

            return JsonSerializer.Deserialize<Character>(charJson.CharacterJsonRepresentation);
        }

        public async Task<ErrorInformation> PutCharacter(string id, Character character) {
            if (String.IsNullOrEmpty(id)) return makeInvalidParameterError();
            if (character == null) return makeInvalidParameterError();

            ErrorInformation result = null;

            CharacterSchema characterData;

            if (CharacterExists(id)) {
                characterData = await Characters.FindAsync(id);
                if (characterData != null) {
                    characterData.CharacterJsonRepresentation = JsonSerializer.Serialize(character);
                }
            }
            else {
                characterData = new CharacterSchema() {
                    Id = character.Id,
                    CharacterJsonRepresentation = JsonSerializer.Serialize(character)
                };

                Characters.Add(characterData);
            }

            try {
                await SaveChangesAsync();
            }
            catch (DbUpdateException) {
                if (CharacterExists(character.Id)) {
                    result = new ErrorInformation() {
                        ErrorCode = 2,
                        ErrorMessage = "Character with this ID already exists."
                    };
                }
                else {
                    throw;
                }
            }

            return result;
        }

        private ErrorInformation makeInvalidParameterError() {
            return new ErrorInformation() {
                ErrorCode = 1,
                ErrorMessage = "Invalid Parameters."
            };
        }

        public bool CharacterExists(string id) {
            return this.Characters.Any((CharacterSchema character) => (character.Id == id));
        }
    }
}