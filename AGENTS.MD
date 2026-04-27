# 使用中文回答问题

- 请用中文回答问题！

# PowerShell 执行规范

- 编译 c++或者 c#工程时先运行环境设置脚本：

```powershell
& "D:/Program Files/Microsoft Visual Studio/2026/Common7/Tools/Launch-VsDevShell.ps1" -Arch amd64 -HostArch amd64;Set-Location D:\Github\XUnity.AutoTranslator
```

- 运行 PowerShell 命令时请直接执行命令，不要使用形如 `pwsh -NoProfile -Command "..."` 的包裹方式。
- 当可执行文件路径需要用引号包裹（例如路径中包含空格）时，在命令最前添加 `& ` 调用运算符。例如：

```powershell
& 'D:/Github/vcpkg/installed/x64-windows/tools/protobuf/protoc.exe' --version
& 'C:/Program Files/Some App/app.exe' /S
```

# C#编译命令

- `msbuild .\XUnity.AutoTranslator.sln /p:Configuration=Release /nologo /verbosity:quiet /clp:Summary`
