using System.Collections;

public interface ITask
{
    IEnumerator Execute();
    void Cancel();
}