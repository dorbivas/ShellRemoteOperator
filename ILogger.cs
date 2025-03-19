using System.Threading.Tasks;

namespace ShellRemoteOperator
{
    public interface ILogger
    {
        Task Log(string message, string level);
    } 
}
