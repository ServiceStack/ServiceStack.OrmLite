SET BUILD=Debug
REM SET BUILD=Release

COPY ..\..\..\ServiceStack\src\ServiceStack.Client\bin\%BUILD%\netstandard1.1\ServiceStack.Client.* .\
COPY ..\..\..\ServiceStack\src\ServiceStack.Interfaces\bin\%BUILD%\netstandard1.1\ServiceStack.Interfaces.* .\

