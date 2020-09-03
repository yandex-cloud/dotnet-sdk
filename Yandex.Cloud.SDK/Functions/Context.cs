namespace Yandex.Cloud.Functions {

public interface Context {
    string RequestId { get; }
    string FunctionName{ get;  }
    string FunctionVersion{ get; }
    int MemoryLimit{ get;  }
    string LogGroupName{ get; }
    string StreamName{ get; }
    string TokenJson{ get; }
}
}
