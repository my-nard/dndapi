using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dndHitpointApi.Models;
using dndHitpointApi.Database;

namespace dndHitpointApi.Controllers {

    // Controller for Hitpoint-related methods
    // General route layout: /characters/{id}/method
    // e.g. /characters/{id}/heal
    // All of the methods are POSTs with their own request class.
    // All the methods respond with a HitpointChangeResponse -> { "error": {ErrorInformation}, "character":{Character} }
    [Route("/characters")]
    [ApiController]
    public class CharacterHitpointController : ControllerBase {
        private readonly CharacterDbContext _characterDbContext;

        public CharacterHitpointController(CharacterDbContext context) {
            _characterDbContext = context;
        }

        // So the heal/tempHP/damage services are all pretty similar in their structure so I did
        // a quick and dirty abstraction of them.
        // characterId - the character GUID
        // validationFunction -> ErrorInformation validationFunction(Character character)
        //     Return an errorInformation object if the char/request are bad
        //     Return null if everything's okay to act on
        // hitpointApplicationFunction -> bool hitpointApplicationFunction(Character)
        //     Return true if there are changes made to the character
        //     Return false if there aren't changes made (and it won't save)
        // Returns a JSON result for a HitpointChangeResponse - basically either an error if
        // there's an error or the updated Character.
        private async Task<JsonResult> GeneralHitpointService(
            string characterId,
            Func<Character, ErrorInformation> validationFunction,
            Func<Character, bool> hitpointApplicationFunction
        ) {
            Character character = await _characterDbContext.GetCharacter(characterId);

            // Heals/TempHP/Damage all return an error and a character, so lets just have 1 response type
            HitpointChangeResponse response = new HitpointChangeResponse();

            // If the caller returns an error, bail out
            response.Error = validationFunction(character);
            if (response.Error != null) {
                return new JsonResult(response); 
            }

            ErrorInformation dbError = null;

            // Have the caller attempt to apply the HP change
            // (I suppose this could be any character object change to generalize it further)
            if (hitpointApplicationFunction(character)) {
                dbError = await _characterDbContext.PutCharacter(character.Id, character);
            }
            
            if (dbError != null) {
                response.Error = dbError;
            }
            else {
                response.Character = character;
            }

            return new JsonResult(response);
        }

        public class HitpointChangeResponse {
            public ErrorInformation Error { get; set; }
            public Character Character { get; set; }
        }




        // The heal and temp HP services have identical signatures so I chose to use the same request
        public class HealCharacterRequest {
            public int Value { get; set; }
        }

        // Route: /characters/{id}/addtemphp
        // Post body JSON: { "value": newTempHpValue }
        // Returns: HitpointChangeResponse as JSON (updated character will be in the response)
        [HttpPost("{id}/addtemphp")]
        public async Task<JsonResult> GrantTempHpToCharacter(string id, HealCharacterRequest healCharacterRequest) {
            return await GeneralHitpointService(
                id,
                (character) => validateHealCharacterRequest(character, healCharacterRequest),
                (character) => character.GrantTemporaryHitpoints(healCharacterRequest.Value)
            );
        }

        // Route: /characters/{id}/heal
        // Post body JSON: { "value": healAmount }
        // Returns: HitpointChangeResponse as JSON (updated character will be in the response)
        [HttpPost("{id}/heal")]
        public async Task<JsonResult> HealCharacter(string id, HealCharacterRequest healCharacterRequest) {
            return await GeneralHitpointService(
                id,
                (character) => validateHealCharacterRequest(character, healCharacterRequest),
                (character) => character.ApplyHealing(healCharacterRequest.Value)
            );
        }

        private ErrorInformation validateHealCharacterRequest(Character character, HealCharacterRequest request) {
            if (character == null) {
                return ErrorInformation.InvalidCharacterError();
            }

            if (request == null || request.Value < 0) {
                return ErrorInformation.InvalidParameterError();
            }
            return null;
        }




        // Route: /characters/{id}/damage
        // Post body JSON:
        //   { "dealtDamage": [ { "value":damageAmount, "type":damageTypeString }, ... ] }
        //
        // DealtDamage is an array of DealtDamageInformation, and the type is a damage type title coming from the client
        // The char model has defense information in it; there's some convenience extensions on the Character class
        // to help manage all that.
        //
        // Returns: HitpointChangeResponse as JSON (updated character will be in the response)
        [HttpPost("{id}/damage")]
        public async Task<JsonResult> DamageCharacter(string id, DamageCharacterRequest damageCharacterRequest) {
            return await GeneralHitpointService(
                id,
                character => validateDamageCharacter(character, damageCharacterRequest),
                character => applyDamageToCharacter(character, damageCharacterRequest.DealtDamage)
            );
        }

        public class DamageCharacterRequest {
            public List<DealtDamageInformation> DealtDamage { get; set; }
        }

        // Go through all the damage in the request and try to apply as much as we can.
        // Return true if we make a change (so we can save)
        private bool applyDamageToCharacter(Character character, List<DealtDamageInformation> dealtDamage) {
            bool anyDamageApplied = false;
            
            dealtDamage.ForEach((damageInformation) => {
                if (character.ApplyDamage(damageInformation)) {
                    anyDamageApplied = true;
                }
            });
            
            return anyDamageApplied;
        }

        private ErrorInformation validateDamageCharacter(Character character, DamageCharacterRequest damageCharacterRequest) {
            if (character == null) {
                return ErrorInformation.InvalidCharacterError();
            }

            if (damageCharacterRequest.DealtDamage == null || damageCharacterRequest.DealtDamage.Count < 1) {
                return ErrorInformation.InvalidParameterError();
            }

            return null;
        }



    }
}
