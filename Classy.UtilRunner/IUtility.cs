namespace Classy.UtilRunner
{
    public interface IUtility
    {
        StatusCode Run(string[] args);
    }

    public enum StatusCode
    {
        Success,
        Failure,
        Warning
    }
}