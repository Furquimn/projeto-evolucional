using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ClosedXML.Excel;

namespace ProjetoEvolucional.Pages
{
    public class ExecutarModel : PageModel
    {
        // verifica se está autenticado, se não, retorna pra index
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Autenticado") != "true")
            {
                return RedirectToPage("Index");
            }
            return Page();
        }

        // Gera 1000 alunos no banco com nomes únicos
        public IActionResult OnPostGerarAlunos()
        {
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=projeto-evolucional;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Não permite executar o código se já houver 1000 alunos
                string sqlVerificaTotal = "SELECT COUNT(*) FROM Alunos";
                using (SqlCommand commandVerificaTotal = new SqlCommand(sqlVerificaTotal, connection))
                {
                    int total = (int)commandVerificaTotal.ExecuteScalar();
                    if (total >= 1000)
                    {
                        TempData["Mensagem"] = "Os alunos já foram gerados anteriormente.";
                        return RedirectToPage("Executar");
                    }
                }

                // Adiciona as matérias na tabela "Disciplina", antes verifica se a Disciplina já existe
                string[] nomesDisciplinas = { "Matemática", "Português", "História", "Geografia", "Inglês", "Biologia", "Filosofia", "Física", "Química" };

                foreach (var nomeDisciplina in nomesDisciplinas)
                {
                    string sqlVerificaDisciplina = "SELECT COUNT(*) FROM Disciplinas WHERE nome = @Nome";
                    using (SqlCommand commandVerificaDisciplina = new SqlCommand(sqlVerificaDisciplina, connection))
                    {
                        commandVerificaDisciplina.Parameters.AddWithValue("@Nome", nomeDisciplina);
                        int count = (int)commandVerificaDisciplina.ExecuteScalar();

                        if (count == 0)
                        {
                            string sqlInsereDisciplina = "INSERT INTO Disciplinas (nome) VALUES (@Nome)";
                            using (SqlCommand commandInsere = new SqlCommand(sqlInsereDisciplina, connection))
                            {
                                commandInsere.Parameters.AddWithValue("@Nome", nomeDisciplina);
                                commandInsere.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // Adiciona os alunos na tabela "Alunos", antes verifica se já tem 1000 alunos adicionados
                string[] nomesBase = { "Ana", "João", "Marcelo", "Felipe", "Maria", "Adriana", "Bruno", "Bianca", "Regiane", "Marcos" };

                foreach (var nome in nomesBase)
                {
                    for (int i = 1; i <= 100; i++)
                    {
                        string nomeCompleto = nome + i.ToString("D3");

                        string sqlInsertAluno = "INSERT INTO Alunos (nome) VALUES (@Nome)";
                        using (SqlCommand commandInsertAluno = new SqlCommand(sqlInsertAluno, connection))
                        {
                            commandInsertAluno.Parameters.AddWithValue("@Nome", nomeCompleto);
                            commandInsertAluno.ExecuteNonQuery();
                        }
                    }

                }

                // Faz a leitura do id dos alunos
                List<int> idsAlunos = new List<int>();

                string sqlSelectIdsAlunos = "SELECT id FROM Alunos";

                using (SqlCommand commandSelectIdsAluno = new SqlCommand(sqlSelectIdsAlunos, connection))
                using (SqlDataReader readerIdsAlunos = commandSelectIdsAluno.ExecuteReader())
                {
                    while (readerIdsAlunos.Read())
                    {
                        int id = readerIdsAlunos.GetInt32(0);
                        idsAlunos.Add(id);
                    }
                }

                // Faz a leitura do id das disciplinas
                List<int> idsDisciplinas = new List<int>();

                string selectIdsDisciplinas = "SELECT id FROM Disciplinas";

                using (SqlCommand commandSelectIdsDisciplinas = new SqlCommand(selectIdsDisciplinas, connection))
                using (SqlDataReader readerIdsDisciplinas = commandSelectIdsDisciplinas.ExecuteReader())
                {
                    while (readerIdsDisciplinas.Read())
                    {
                        int id = readerIdsDisciplinas.GetInt32(0);
                        idsDisciplinas.Add(id);
                    }
                }

                // Gera uma nota aleatoria com duas casas decimais que pode ser entre 00.00 e 10.00
                Random notaAleatoria = new Random();

                foreach (var idAluno in idsAlunos)
                {
                    foreach (var idDisciplina in idsDisciplinas)
                    {
                        double notaGerada = Math.Round(notaAleatoria.NextDouble() * 10, 2);

                        string sqlInsertNota = "INSERT INTO Notas (IdAluno, IdDisciplina, Nota) VALUES (@IdAluno, @IdDisciplina, @Nota)";

                        using (SqlCommand commandInsertNota = new SqlCommand(sqlInsertNota, connection))
                        {
                            commandInsertNota.Parameters.AddWithValue("@IdAluno", idAluno);
                            commandInsertNota.Parameters.AddWithValue("@IdDisciplina", idDisciplina);
                            commandInsertNota.Parameters.AddWithValue("@Nota", notaGerada);
                            commandInsertNota.ExecuteNonQuery();
                        }
                    }
                }
            }

            // Exibe mensagem quando gerar os alunos e redireciona para a página Executar para impedir que continue rodando.
            TempData["Mensagem"] = "Alunos e notas gerados com sucesso.";
            TempData["ExcelGerado"] = false;

            return RedirectToPage("Executar");
        }

        // Permite fazer download do Arquivo Excel com a planilha dos alunos e respectivas notas e médias
        public IActionResult OnPostGerarExcel()
        {
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=projeto-evolucional;Trusted_Connection=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT A.nome AS Aluno, D.nome AS Disciplina, N.Nota
            FROM Notas N
            JOIN Alunos A ON A.id = N.IdAluno
            JOIN Disciplinas D ON D.id = N.IdDisciplina
            ORDER BY A.nome, D.nome";

                using (SqlCommand command = new SqlCommand(sql, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Relatório");

                    ws.Cell(1, 1).Value = "Aluno";
                    ws.Cell(1, 2).Value = "Disciplina";
                    ws.Cell(1, 3).Value = "Nota";

                    int row = 2;
                    while (reader.Read())
                    {
                        ws.Cell(row, 1).Value = reader["Aluno"].ToString();
                        ws.Cell(row, 2).Value = reader["Disciplina"].ToString();
                        ws.Cell(row, 3).Value = Convert.ToDouble(reader["Nota"]);
                        row++;
                    }

                    ws.Range("A1:C1").Style.Font.Bold = true;
                    ws.Columns("A:C").AdjustToContents();

                    // Salva em memória temporária (como array de bytes na sessão)
                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        HttpContext.Session.Set("ArquivoExcel", stream.ToArray());
                    }
                }
            }

            TempData["Mensagem"] = "Arquivo Excel gerado com sucesso. Clique abaixo para fazer o download.";
            TempData["ExcelGerado"] = true;

            return RedirectToPage("Executar");
        }

        public IActionResult OnGetDownloadExcel()
        {
            if (!HttpContext.Session.TryGetValue("ArquivoExcel", out var arquivo))
            {
                TempData["Mensagem"] = "Arquivo não encontrado. Gere o relatório novamente.";
                return RedirectToPage("Executar");
            }

            return File(arquivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RelatorioNotas.xlsx");
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Remove("Autenticado");
            return RedirectToPage("Index");
        }
    }
}
