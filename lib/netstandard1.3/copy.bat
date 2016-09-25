SET BUILD=Debug
REM SET BUILD=Release

COPY ..\..\..\ServiceStack.Text\src\ServiceStack.Text\bin\%BUILD%\netstandard1.3\ServiceStack.Text.* .\
COPY ..\..\..\ServiceStack\src\ServiceStack.Common\bin\%BUILD%\netstandard1.3\ServiceStack.Common.* .\

