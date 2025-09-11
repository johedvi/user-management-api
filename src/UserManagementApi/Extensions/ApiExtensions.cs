using UserManagementApi.Models.User;
using UserManagementApi.Services.UserService;
using FluentValidation;

namespace UserManagementApi.Extensions;

public static class ApiExtensions
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "API is running");
    }

    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth")
                     .WithTags("Authentication");

        auth.MapPost("/register", RegisterUser)
            .WithName("Register")
            .WithSummary("Register a new user")
            .Produces<object>(200)
            .Produces<object>(400);

        auth.MapPost("/login", LoginUser)
            .WithName("Login")
            .WithSummary("Login user")
            .Produces<object>(200)
            .Produces<object>(400)
            .Produces(401);
    }

    public static void MapUserEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("/api/users")
                      .RequireAuthorization()
                      .WithTags("Users");

        users.MapGet("", GetAllUsers)
             .WithName("GetAllUsers")
             .WithSummary("Get all users")
             .Produces<List<GetUserResponse>>(200);

        users.MapGet("{id:int}", GetUserById)
             .WithName("GetUserById")
             .WithSummary("Get user by ID")
             .Produces<GetUserResponse>(200)
             .Produces(404);

        users.MapPost("", CreateUser)
             .WithName("CreateUser")
             .WithSummary("Create a new user")
             .Produces<GetUserResponse>(201)
             .Produces<object>(400);

        users.MapPut("{id:int}", UpdateUser)
             .WithName("UpdateUser")
             .WithSummary("Update user")
             .Produces<GetUserResponse>(200)
             .Produces(404)
             .Produces<object>(400);

        users.MapDelete("{id:int}", DeleteUser)
             .WithName("DeleteUser")
             .WithSummary("Delete user")
             .RequireAuthorization(policy => policy.RequireRole("Admin"))
             .Produces(204)
             .Produces(404);
    }

    private static async Task<IResult> RegisterUser(AddUserRequest request, IUserService userService, IValidator<AddUserRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { message = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
        }

        try
        {
            var token = await userService.Register(request);
            return Results.Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> LoginUser(LoginRequest request, IUserService userService, IValidator<LoginRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { message = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
        }

        var token = await userService.Login(request);
        return token != null 
            ? Results.Ok(new { token })
            : Results.Unauthorized();
    }

    private static async Task<IResult> GetAllUsers(IUserService userService)
    {
        var users = await userService.GetAllUsers();
        return Results.Ok(users);
    }

    private static async Task<IResult> GetUserById(int id, IUserService userService)
    {
        var user = await userService.GetUserById(id);
        return user != null ? Results.Ok(user) : Results.NotFound();
    }

    private static async Task<IResult> CreateUser(AddUserRequest request, IUserService userService, IValidator<AddUserRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { message = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
        }

        try
        {
            var user = await userService.AddUser(request);
            return Results.Created($"/api/users/{user.Id}", user);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateUser(int id, UpdateUserRequest request, IUserService userService, IValidator<UpdateUserRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { message = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
        }

        try
        {
            var user = await userService.UpdateUser(id, request);
            return user != null ? Results.Ok(user) : Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteUser(int id, IUserService userService)
    {
        var deleted = await userService.DeleteUser(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}