using System.Net.Http;
using System.Threading.Tasks;
using activityhistory_application.Interfaces;

namespace activityhistory_infrastructure.Services
{
    /// Clase Service que se encarga de procesar todas las operaciones sobre un usuario, realizando peticiones HTTP al Microservicio Usuarios.
    public class UserService : IUserServices
    {
        /// <summary>
        /// Atributo que se encarga de procesar las solicitudes a servicios externos.
        /// </summary>
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<Guid> GetIdUserByEmailServices(string email)
        {
            Console.WriteLine("Iniciando solicitud para obtener ID de usuario por correo...");
            var response = await _httpClient.GetAsync($"http://localhost:7181/api/users/getIdUser/{email}");
            Console.WriteLine("Solicitud completada.");
            if (!response.IsSuccessStatusCode)
            {
                return Guid.Empty;
            }
            Console.WriteLine($"Guid: {response}");
            var guidString = await response.Content.ReadAsStringAsync();

            if (Guid.TryParse(guidString.Trim('"'), out Guid userId))
            {
                return userId;
            }
            else
            {
                return Guid.Empty;
            }
        }




    }
}