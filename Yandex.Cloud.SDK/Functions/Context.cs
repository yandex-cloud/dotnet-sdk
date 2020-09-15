namespace Yandex.Cloud.Functions {

public interface Context {
    string RequestId { get; }
    string FunctionId{ get;  }
    string FunctionVersion{ get; }
    int MemoryLimitInMB{ get; }
    string LogGroupName{ get; }
    string StreamName{ get; }
    string TokenJson{ get; }
}
}
