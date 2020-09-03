namespace Yandex.Cloud.Functions {

public interface YcFunction<T, R> {
    R FunctionHandler(T request, Context context);
}
}
