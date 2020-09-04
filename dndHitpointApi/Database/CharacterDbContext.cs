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

        public DbSet<CharacterSchema> Characters { get; set; }
        
        // Gets a character object from the DB; abstract away the JSON from the controller a bit
        public async Task<Character> GetCharacter(string id) {
            CharacterSchema charJson = await Characters.FindAsync(id);

            if (charJson == null) return null;
            if (String.IsNullOrEmpty(charJson.CharacterJsonRepresentation)) return null;

            return JsonSerializer.Deserialize<Character>(charJson.CharacterJsonRepresentation);
        }

        // Puts a character into the DB
        // ID and character.ID are a little redundant, not sure how I feel about that
        // I figure we might want a char copy function at some point, so maybe it makes sense to
        // apply the id to the char as we put them, but I'd probably wanna figure out use cases first
        public async Task<ErrorInformation> PutCharacter(string id, Character character) {
            if (String.IsNullOrEmpty(id)) return ErrorInformation.InvalidParameterError();
            if (character == null) return ErrorInformation.InvalidParameterError();

            if (id == null) return ErrorInformation.InvalidParameterError();
            if (!id.Equals(character.Id)) return ErrorInformation.InvalidParameterError();

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
                    result = ErrorInformation.DuplicateCharacterError();
                }
                else {
                    throw;
                }
            }

            return result;
        }

        // Check to see if the DB has any chars with the given ID
        public bool CharacterExists(string id) {
            return Characters.Any((CharacterSchema character) => (character.Id == id));
        }
    }
}