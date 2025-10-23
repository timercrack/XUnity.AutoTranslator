# 使用中文回答问题

- 请用中文回答问题！

# 代码搜索规范和资源路径

- 不要使用内置的搜索工具，而是直接调用 `select-string` 搜索，用通配符时只在文件上用，不要在目录上用，比如可以匹配`src/*.cpp`，不要去匹配`src/**/*.cpp`，示例：

```powershell
select-string -Path D:/Github/bighelper_dll/src/*.cpp -Pattern 'some_function'
```

- 雀魂游戏 LUA 路径： `D:/Github/bighelper_dll/backup/out/lua/MajsoulCN/*.lua`
- 一番街游戏 LUA 路径： `D:/Github/bighelper_dll/backup/out/lua/riichicity/*.lua`

# PowerShell 执行规范

- 编译 c++或者 c#工程时先运行环境设置脚本：

```powershell
& "D:/Program Files/Microsoft Visual Studio/2026/Common7/Tools/Launch-VsDevShell.ps1" -Arch amd64 -HostArch amd64
```

- 运行 PowerShell 命令时请直接执行命令，不要使用形如 `pwsh -NoProfile -Command "..."` 的包裹方式。
- 当可执行文件路径需要用引号包裹（例如路径中包含空格）时，在命令最前添加 `& ` 调用运算符。例如：

```powershell
& 'D:/Github/vcpkg/installed/x64-windows/tools/protobuf/protoc.exe' --version
& 'C:/Program Files/Some App/app.exe' /S
```

# Python 脚本执行规范

- 必须使用指定虚拟环境的 Python 解释器运行脚本： `D:/Github/venv313/Scripts/python.exe`

调用示例：

```powershell
& 'D:/Github/venv313/Scripts/python.exe' your_script.py
```

- 不要直接用系统自带的 python 或 python3 命令，以避免依赖冲突。

- 如果脚本路径中包含空格，必须用单引号 '...' 包裹。

# C++ 编码规范

- C++ 代码尽量使用最新的 MSVC 支持的标准。
- DLL 里面的所有代码都不要加`extern "C"`与`__declspec(dllexport)`，所有函数都是 DLL 内部使用，都是 C++实现。
- 不要做没必要的空指针检测和多余的{}包裹，保持代码简洁、减少缩进。
- 全局变量在 `config.cpp/.h` 中的 `Config` 结构定义
- 日志必须以当前文件名（去掉扩展名），作为前缀，例如 `socket_client.cpp` 的 tag 是 `[socket_client]`

# C++ 代码章节顺序规范

- Includes（分组：Project -> Third-party -> System/STL）
- Forward Declarations / Typedefs
- Static State / 常量
- Internal Helpers（纯内部工具函数）
- Core / Hook Bodies / 主要逻辑
- Public Interface（对外被其它编译单元调用的函数）

# C++ 变量命名规范

- 文件内部静态变量：使用 `m_` 前缀
- 函数局部/临时变量：常规驼峰命名，无前缀
- 全局变量使用 `g_` 前缀
- 常量使用大写下划线命名

# JSON 序列化/反序列化

- 使用 `nlohmann::json` 进行所有的 JSON 序列化/反序列化。
- 对结构体的 JSON 序列化/反序列化，使用 `NLOHMANN_DEFINE_TYPE_INTRUSIVE` 宏定义，不要自己写 `to_json`/`from_json` 函数，更不要手动一条一条赋值。

# Lua 编码规范

- Lua 脚本内部的函数和变量以 `bh_` 线开头。
- Lua 脚本对外接口的函数和变量以 `BH_` 开头。
- Lua 函数内不要做多余的函数参数和函数返回值的合法性、nil 检验，不要用 pcall 包裹函数调用。

# rust 项目构建规范

- 只允许使用 `cargo build` 构建 debug 版本

# javascript 编码规范

- javascript 代码不要加任何 try/catch，出错直接抛到外层由浏览器处理

# rust 编码规范

- rust 在返回值是 Result 的函数内部尽量用?，不要编写额外的类型检查，将错误直接暴露给调用方

# golang 编码规范

- 用最新的 Go 版本（当前是 1.25）

# C#编译命令

- `msbuild .\XUnity.AutoTranslator.sln /p:Configuration=Release /nologo /verbosity:quiet /clp:Summary`
