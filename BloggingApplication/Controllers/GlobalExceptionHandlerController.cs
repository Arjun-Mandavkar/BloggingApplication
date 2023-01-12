using BloggingApplication.CustomExceptions;
using BloggingApplication.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;

namespace BloggingApplication.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GlobalExceptionHandlerController : ControllerBase
    {
        [Route("/ErrorDevEnv")]
        public ApiDevResponseDto GlobalDevExceptionHandler()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error; //Exception which is thrown

            if(exception == null)
                return new ApiDevResponseDto(new Exception("Something went wrong."));

            // Assign respose code according to exception
            Response.StatusCode = ExceptionStatusCodeMapper(exception);

            return new ApiDevResponseDto(exception);
        }

        [Route("/Error")]
        public ApiResponseDto GlobalExceptionHandler()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error; //Exception which is thrown

            if (exception == null)
                return new ApiResponseDto(new Exception("Something went wrong."));

            // Assign respose code according to exception
            Response.StatusCode = ExceptionStatusCodeMapper(exception);
            return new ApiResponseDto(exception);
        }

        private int ExceptionStatusCodeMapper(Exception exception)
        {
            if (exception is SqlException) return (int)HttpStatusCode.BadRequest;
            else if (exception is InvalidOperationException) return(int)HttpStatusCode.BadRequest;
            else if (exception is UserRegistrationException) return (int)HttpStatusCode.BadRequest;
            else if (exception is UserLoginException) return (int)HttpStatusCode.BadRequest;
            else if (exception is UserCrudException) return (int)HttpStatusCode.BadRequest;
            else if (exception is BlogCrudException) return (int)HttpStatusCode.BadRequest;
            else if (exception is BlogOperationException) return (int)HttpStatusCode.BadRequest;
            else if (exception is UnauthorizedUserException) return (int)HttpStatusCode.Forbidden;
            else return (int)HttpStatusCode.InternalServerError;    // Internal Server Error by default
        }
    }
}
