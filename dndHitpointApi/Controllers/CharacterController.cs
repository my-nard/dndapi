using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dndHitpointApi.Models;
using dndHitpointApi.Database;

namespace dndHitpointApi.Controllers {
    
    // CharacterController - I'm using this to manage character records
    // So there are new/get/delete services (didn't do update but it would be straightforward)
    // I threw the HP management services in the hitpoint controller
    [Route("/characters")]
    [ApiController]
    public class CharacterController : ControllerBase {
        private readonly CharacterDbContext _characterDbContext;

        public CharacterController(CharacterDbContext context) {
            _characterDbContext = context;
        }



        // Route: /characters/new
        // Post body should be a character JSON object (though it'd likely be a good idea to create an entry point request class)
        // We'll assign it a new ID, if it has one (this effectively copies the character into a new record)
        // The response is an internal class - See NewCharacterResponse right below this
        // (I tend to prefer app errors to HTML response codes for stuff like this - I imagine
        //  a lot of the errors we could run into here are with inappropriately filled out
        //  character "sheets" - I'd save the HTTP errors for when I think the client isn't playing nice)
        // The caller will need to grab the id property in the char response and use that for subsequent API calls
        [HttpPost("new")]
        public async Task<JsonResult> NewCharacter(Character character) {
            if (!validateNewCharacter(character)) {
                return new JsonResult(new NewCharacterResponse() {
                    Error = new ErrorInformation() {
                        ErrorCode = 1,
                        ErrorMessage = "Invalid character"
                    }
                });
            }
            
            NewCharacterResponse response = new NewCharacterResponse();

            ErrorInformation dbError = await _characterDbContext.PutCharacter(character.Id, character);

            if (dbError != null) {
                response.Error = dbError;
            }
            else {
                response.Character = character;
            }

            return new JsonResult(response);
        }
        
        // Just used above to encapsulate an error code for the client along with the character JSON
        // Figured we'd be better off with returning both all the time in case we had warnings (e.g. "this char is okay but you need to fill out your race")
        public class NewCharacterResponse {
            public ErrorInformation Error { get; set; }
            
            // Provides the caller with a validated/massaged character
            public Character Character { get; set; }
        }

        private bool validateNewCharacter(Character character) {
            // I figure there'd be more validation here, and maybe we'd want to return a validation reason
            // For now I'm just using simple guards for testing purposes
            if (character == null) return false;
            if (String.IsNullOrEmpty(character.Name)) return false;

            // Just going to overwrite the ID with a guid for now - we'd want to double check conflicts but this is probably fine for this exercise
            character.Id = System.Guid.NewGuid().ToString();

            // Find the maximum hitpoints for the character
            // We might want to else this and recalculate the client's HP if it's giving them to us at some point
            // Or potentially if it passes in garbage (e.g. negative HP aren't a thing anymore) we could return false
            if (character.HitPoints == null) {
                character.HitPoints = character.CalculateMaximumHitpoints();

                // Might as well long rest...
                character.HitPoints.Current = character.HitPoints.Maximum;
                character.HitPoints.Temporary = 0;
            }

            return true;
        }




        // Route: /characters/{id}
        // HTTP Get - returns the JSON for a character
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCharacter(string id) {
            CharacterSchema character = await _characterDbContext.Characters.FindAsync(id);

            if (character == null) {
                return NotFound();
            }

            return Content(character.CharacterJsonRepresentation, "application/json");
        }





        // Route: /characters/delete/{id}
        // HTTP Delete - deletes the character with the given id
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCharacter(string id) {
            var character = await _characterDbContext.Characters.FindAsync(id);

            if (character == null) {
                return NotFound();
            }

            _characterDbContext.Characters.Remove(character);
            await _characterDbContext.SaveChangesAsync();
            
            return Content(character.CharacterJsonRepresentation, "application/json");
        }
    }
}
