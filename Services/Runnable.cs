namespace Outils;
public class StartStop
{
    private bool isRunning = false;
    public bool IsRunning => isRunning;
    public void Start() => isRunning = true;
    public void Stop() => isRunning = false;
}