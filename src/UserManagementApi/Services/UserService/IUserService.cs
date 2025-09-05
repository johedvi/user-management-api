using UserManagementApi.Models.User;

namespace UserManagementApi.Services.UserService;
    public interface IUserService {
        Task<List<GetUserResponse>> GetAllUsers();
        Task<GetUserResponse?> GetUserById(int id);
        Task<GetUserResponse> AddUser(AddUserRequest newUser);
        Task<GetUserResponse?> UpdateUser(int id, UpdateUserRequest updatedUser);
        Task<bool> DeleteUser(int id);
        Task<string> Register(AddUserRequest request);
        Task<string?> Login(LoginRequest request);
    }

