using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace ProjetoEvolucional.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnPost(string Login, string Senha)
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Senha))
            {
                TempData["ErroLogin"] = "Por favor, preencha todos os campos.";
                return Page();
            }

            string connectionString = "Server=localhost\\SQLEXPRESS;Database=projeto-evolucional;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlSelectUsuarios = "SELECT COUNT(*) FROM Usuarios WHERE Login = @Login AND Senha = @Senha";

                using (SqlCommand commandSelectUsuarios = new SqlCommand(sqlSelectUsuarios, connection))
                {
                    commandSelectUsuarios.Parameters.AddWithValue("@Login", Login);
                    commandSelectUsuarios.Parameters.AddWithValue("@Senha", Senha);

                    int count = (int)commandSelectUsuarios.ExecuteScalar();

                    if (count > 0)
                    {
                        HttpContext.Session.SetString("Autenticado", "true");
                        return RedirectToPage("Executar");
                    }
                }
            }

            TempData["ErroLogin"] = "Usuário ou senha inválidos.";
            return Page();
        }


    }
}
