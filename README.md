# Projeto Evolucional

Sistema de geração de alunos, atribuição de notas e exportação de relatório em Excel.

---

## 🔧 Tecnologias utilizadas

- ASP.NET Core Razor Pages (.NET 9)
- SQL Server (local)
- ClosedXML (para geração de planilhas Excel)
- Sessões (autenticação)

---

## 🚀 Funcionalidades

- Login com autenticação via banco de dados
- Geração de 1000 alunos automaticamente
- Criação automática de 9 disciplinas
- Atribuição aleatória de notas (0 a 10) para cada aluno em cada disciplina
- Exportação do boletim para Excel com média por aluno
- Mensagens de feedback na interface
- Controle de sessão com botão de logout

---

## 🔐 Acesso

Login padrão já inserido no banco de dados:

- **Usuário**: `candidato-evolucional`
- **Senha**: `123456`

---

## 💻 Como rodar o projeto localmente

### Pré-requisitos

- Visual Studio 2022+ ou VS Code com .NET SDK 9 instalado
- SQL Server LocalDB ou instância local do SQL Server
- Git

### Passos

```bash
git clone https://github.com/Furquimn/projeto-evolucional.git
cd projeto-evolucional
dotnet restore
dotnet run
