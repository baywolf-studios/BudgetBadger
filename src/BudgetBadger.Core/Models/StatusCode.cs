using System;
namespace BudgetBadger.Logic.Models
{
    public enum StatusCode
    {
        // Successful
        OK = 200,
        Created = 201,
        NoContent = 204,

        // Failures
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        Conflict = 409,
        Gone = 410,
        InternalError = 500,
        NotImplemented = 501,
    }
}

