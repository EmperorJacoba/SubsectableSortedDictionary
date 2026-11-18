namespace SubsectableSortedDictionary.Exceptions;

[Serializable]
public class SSSDInternalNodeException : Exception
{
    public SSSDInternalNodeException() {}
    public SSSDInternalNodeException(string message) : base(message) {}
    public SSSDInternalNodeException(string message, Exception innerException) : base(message, innerException) {}
}