using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.User.Register;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;

namespace MyRecipeBook.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersConttroler : ControllerBase {

  [HttpPost]
  [ProducesResponseType(typeof(ResponseRegisterUserJson), StatusCodes.Status201Created)] // Swagger documentation for successful response
  [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)] // Swagger documentation for bad request response
  public async Task<IActionResult> Register(
      [FromBody] RequestRegisterUserAccountJson request,
      [FromServices] IRegisterUserAccountUseCase useCase
      ) {
    var result = await useCase.Execute(request);
    return Created(string.Empty, result);
  }
}
