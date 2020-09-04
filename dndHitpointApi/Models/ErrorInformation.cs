namespace dndHitpointApi.Models {
    public class ErrorInformation {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }


        public static ErrorInformation InvalidParameterError() {
            return new ErrorInformation() {
                ErrorCode = 1,
                ErrorMessage = "Invalid Parameters."
            };
        }

        public static ErrorInformation DuplicateCharacterError() {
            return new ErrorInformation() {
                ErrorCode = 2,
                ErrorMessage = "Character with this ID already exists."
            };
        }

        public static ErrorInformation InvalidCharacterError() {
            return new ErrorInformation() {
                ErrorCode = 3,
                ErrorMessage = "Invalid character"
            };
        }
        
    }
}