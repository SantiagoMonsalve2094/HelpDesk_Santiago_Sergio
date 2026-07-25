namespace HelpDesk.Backend.Api.Resources;

public static class ApiMessages
{
    public const string InvalidCredentialsTitle = "Credenciales inválidas";
    public const string AccessDeniedTitle = "Acceso denegado";
    public const string ResourceNotFoundTitle = "Recurso no encontrado";
    public const string DomainConflictTitle = "Conflicto de dominio";
    public const string ConcurrencyConflictTitle = "Conflicto de concurrencia";
    public const string ConcurrencyConflict =
        "El recurso fue modificado por otra operación. Actualice la información e inténtelo de nuevo.";
    public const string UniquenessConflictTitle = "Conflicto de unicidad";
    public const string UniquenessConflict =
        "Ya existe un registro con los datos únicos indicados.";
    public const string ConflictTitle = "Conflicto";
    public const string InvalidRequestTitle = "Solicitud inválida";
    public const string InvalidRequest = "La solicitud contiene datos inválidos.";
    public const string UnexpectedErrorTitle = "Error interno";
    public const string UnexpectedError = "Ocurrió un error inesperado.";
    public const string ValidationTitle = "Error de validación";
    public const string Validation = "Uno o más campos son inválidos.";
    public const string ActorClaimRequired =
        "El token no contiene un identificador de usuario válido.";
    public const string InvalidFieldValue = "El valor enviado no es válido.";
    public const string InvalidFields = "Uno o más campos de la solicitud son inválidos.";
    public const string UnauthenticatedTitle = "No autenticado";
    public const string ValidAccessTokenRequired = "Se requiere un token de acceso válido.";
    public const string OperationNotAllowed =
        "El usuario no tiene permisos para ejecutar esta operación.";
    public const string TooManyAttemptsTitle = "Demasiados intentos";
    public const string TooManyLoginAttempts =
        "Se permiten máximo cinco intentos de inicio de sesión por minuto.";
    public const string SwaggerBearerDescription =
        "Ingrese el token JWT con el prefijo Bearer.";
}
